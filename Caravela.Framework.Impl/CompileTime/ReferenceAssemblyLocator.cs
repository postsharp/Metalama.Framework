// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Impl.Options;
using Caravela.Framework.Impl.Utilities;
using Caravela.Framework.Sdk;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Caravela.Framework.Impl.CompileTime
{
    /// <summary>
    /// Provides the location to the reference assemblies that are needed to create the compile-time projects.
    /// This is achieved by creating an MSBuild project and restoring it.
    /// </summary>
    public class ReferenceAssemblyLocator
    {
        private const string _frameworkAssemblyName = "Caravela.Framework";
        private readonly string _cacheDirectory;
        
        /// <summary>
        /// Gets the name (without path and extension) of Caravela assemblies.
        /// </summary>
        private ImmutableArray<string> CaravelaImplementationAssemblyNames { get; } = ImmutableArray.Create(
            "Caravela.Framework.Sdk",
            "Caravela.Framework.Impl" );

        /// <summary>
        /// Gets the name (without path and extension) of all standard assemblies, including Caravela, Roslyn and .NET standard.
        /// </summary>
        public ImmutableHashSet<string> StandardAssemblyNames { get; }

        /// <summary>
        /// Gets the full path of system assemblies (.NET Standard and Roslyn). 
        /// </summary>
        public ImmutableArray<string> SystemAssemblyPaths { get; }

        /// <summary>
        /// Gets the name (without path and extension) of system assemblies (.NET Standard and Roslyn). 
        /// </summary>
        public ImmutableHashSet<string> SystemAssemblyNames { get; }

        /// <summary>
        /// Gets the full path of all standard assemblies, including Caravela, Roslyn and .NET standard.
        /// </summary>
        public ImmutableArray<MetadataReference> StandardCompileTimeMetadataReferences { get; }

        public ReferenceAssemblyLocator( IServiceProvider serviceProvider )
        {
            this._cacheDirectory = serviceProvider.GetService<IDirectoryOptions>().AssemblyLocatorCacheDirectory;

            this.SystemAssemblyPaths = this.GetSystemAssemblyPaths().ToImmutableArray();

            this.SystemAssemblyNames = this.SystemAssemblyPaths
                .Select( Path.GetFileNameWithoutExtension )
                .ToImmutableHashSet( StringComparer.OrdinalIgnoreCase );

            this.StandardAssemblyNames = this.CaravelaImplementationAssemblyNames
                .Concat( _frameworkAssemblyName )
                .Concat( this.SystemAssemblyPaths )
                .ToImmutableHashSet( StringComparer.OrdinalIgnoreCase );

            // Make sure all necessary assemblies are loaded in the current AppDomain.
            _ = new AspectWeaverAttribute( null! );
            _ = meta.CompileTime<object>( null );

            // Get our public API assembly in its .NET Standard 2.0 build.
            var frameworkAssemblyReference = (MetadataReference)
                MetadataReference.CreateFromStream( this.GetType().Assembly.GetManifestResourceStream( _frameworkAssemblyName + ".dll" ) );

            // Get implementation assembly paths from the current AppDomain
            var caravelaImplementationPaths = AppDomain.CurrentDomain.GetAssemblies()
                .Where( a => !a.IsDynamic ) // accessing Location of dynamic assemblies throws
                .Select( a => a.Location )
                .Where( path => this.CaravelaImplementationAssemblyNames.Contains( Path.GetFileNameWithoutExtension( path ) ) )
                .ToList();

            // Assert that we found everything we need, because debugging is difficult when this step goes wrong.
            foreach ( var assemblyName in this.CaravelaImplementationAssemblyNames )
            {
                if ( !caravelaImplementationPaths.Any( a => a.EndsWith( assemblyName + ".dll", StringComparison.OrdinalIgnoreCase ) ) )
                {
                    throw new AssertionFailedException( $"Cannot find {assemblyName}." );
                }
            }

            this.StandardCompileTimeMetadataReferences =
                this.SystemAssemblyPaths
                    .Concat( caravelaImplementationPaths )
                    .Select( c => (MetadataReference) MetadataReference.CreateFromFile( c ) )
                    .Prepend( frameworkAssemblyReference )
                    .ToImmutableArray();
        }

        private IEnumerable<string> GetSystemAssemblyPaths()
        {
            var metadataReader = AssemblyMetadataReader.GetInstance( typeof(ReferenceAssemblyLocator).Assembly );

            var projectText =
                $@"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include='Microsoft.CSharp' Version='{metadataReader.GetPackageVersion( "Microsoft.CSharp" )}' />
    <PackageReference Include='Microsoft.CodeAnalysis.CSharp' Version='{metadataReader.GetPackageVersion( "Microsoft.CodeAnalysis.CSharp" )}' />
    <PackageReference Include='System.Collections.Immutable' Version='{metadataReader.GetPackageVersion( "System.Collections.Immutable" )}' />
  </ItemGroup>
  <Target Name='WriteReferenceAssemblies' DependsOnTargets='FindReferenceAssembliesForReferences'>
    <WriteLinesToFile File='assemblies.txt' Overwrite='true' Lines='@(ReferencePathWithRefAssemblies)' />
  </Target>
</Project>";

            var tempProjectDirectory = Path.Combine( this._cacheDirectory, nameof(ReferenceAssemblyLocator) );

            using var mutex = MutexHelper.WithGlobalLock( tempProjectDirectory );
            var referenceAssemblyListFile = Path.Combine( tempProjectDirectory, "assemblies.txt" );

            if ( File.Exists( referenceAssemblyListFile ) )
            {
                var referenceAssemblies = File.ReadAllLines( referenceAssemblyListFile );

                if ( referenceAssemblies.All( File.Exists ) )
                {
                    return referenceAssemblies;
                }
            }

            Directory.CreateDirectory( tempProjectDirectory );

            File.WriteAllText( Path.Combine( tempProjectDirectory, "TempProject.csproj" ), projectText );

            var psi = new ProcessStartInfo( "dotnet", "build -t:WriteReferenceAssemblies" )
            {
                WorkingDirectory = tempProjectDirectory, UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true
            };

            var process = Process.Start( psi ).AssertNotNull();

            var lines = new List<string>();
            process.OutputDataReceived += ( _, e ) => lines.Add( e.Data );

            process.BeginOutputReadLine();
            process.WaitForExit();

            if ( process.ExitCode != 0 )
            {
                throw new InvalidOperationException(
                    "Error while building temporary project to locate reference assemblies:" + Environment.NewLine
                                                                                             + string.Join( Environment.NewLine, lines ) );
            }

            return File.ReadAllLines( referenceAssemblyListFile );
        }
    }
}