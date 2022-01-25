// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Collections;
using Metalama.Framework.Engine.Options;
using Metalama.Framework.Engine.Sdk;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Project;
using Metalama.Framework.RunTime;
using Microsoft.CodeAnalysis;
using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Extensibility;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Metalama.Framework.Engine.CompileTime
{
    /// <summary>
    /// Provides the location to the reference assemblies that are needed to create the compile-time projects.
    /// This is achieved by creating an MSBuild project and restoring it.
    /// </summary>
    internal class ReferenceAssemblyLocator : IService
    {
        private const string _compileTimeFrameworkAssemblyName = "Metalama.Framework";
        private readonly string _cacheDirectory;
        private readonly ILogger _logger;

        /// <summary>
        /// Gets the name (without path and extension) of Metalama assemblies.
        /// </summary>
        private ImmutableArray<string> MetalamaImplementationAssemblyNames { get; } = ImmutableArray.Create(
            "Metalama.Framework.Sdk",
            "Metalama.Framework.Engine" );

        /// <summary>
        /// Gets the name (without path and extension) of all standard assemblies, including Metalama, Roslyn and .NET standard.
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

        public bool IsSystemAssemblyName( string assemblyName )
            => string.Equals( assemblyName, "System.Private.CoreLib", StringComparison.OrdinalIgnoreCase )
               || this.SystemAssemblyNames.Contains( assemblyName );

        public bool IsStandardAssemblyName( string assemblyName )
            => string.Equals( assemblyName, "System.Private.CoreLib", StringComparison.OrdinalIgnoreCase )
               || this.StandardAssemblyNames.Contains( assemblyName );

        /// <summary>
        /// Gets the full path of all standard assemblies, including Metalama, Roslyn and .NET standard.
        /// </summary>
        public ImmutableArray<MetadataReference> StandardCompileTimeMetadataReferences { get; }

        public ReferenceAssemblyLocator( IServiceProvider serviceProvider )
        {
            this._cacheDirectory = serviceProvider.GetRequiredService<IPathOptions>().AssemblyLocatorCacheDirectory;
            this._logger = serviceProvider.GetLoggerFactory().CompileTime();

            this.SystemAssemblyPaths = this.GetSystemAssemblyPaths().ToImmutableArray();

            this.SystemAssemblyNames = this.SystemAssemblyPaths
                .Select( Path.GetFileNameWithoutExtension )
                .ToImmutableHashSet( StringComparer.OrdinalIgnoreCase );

            this.StandardAssemblyNames = this.MetalamaImplementationAssemblyNames
                .Concat( _compileTimeFrameworkAssemblyName )
                .Concat( this.SystemAssemblyPaths.Select( Path.GetFileNameWithoutExtension ) )
                .ToImmutableHashSet( StringComparer.OrdinalIgnoreCase );

            // Make sure all necessary assemblies are loaded in the current AppDomain.
            _ = new AspectWeaverAttribute( null! );
            _ = new[] { 1, 2, 3 }.Buffer();

            // Get our public API assembly in its .NET Standard 2.0 build.
            var currentAssembly = this.GetType().Assembly;

            var frameworkAssemblyReference = (MetadataReference)
                MetadataReference.CreateFromStream(
                    currentAssembly.GetManifestResourceStream( _compileTimeFrameworkAssemblyName + ".dll" ),
                    filePath: $"[{currentAssembly.Location}]{_compileTimeFrameworkAssemblyName}.dll" );

            // Get implementation assembly paths from the current AppDomain. We need to match our exact version number.
            var metalamaImplementationPaths = AppDomain.CurrentDomain.GetAssemblies()
                .Where( a => !a.IsDynamic ) // accessing Location of dynamic assemblies throws
                .Where(
                    a => this.MetalamaImplementationAssemblyNames.Contains( Path.GetFileNameWithoutExtension( a.Location ) ) &&
                         AssemblyName.GetAssemblyName( a.Location ).Version == currentAssembly.GetName().Version )
                .Select( a => a.Location )
                .ToList();

            // Assert that we found everything we need, because debugging is difficult when this step goes wrong.
            foreach ( var assemblyName in this.MetalamaImplementationAssemblyNames )
            {
                if ( !metalamaImplementationPaths.Any( a => a.EndsWith( assemblyName + ".dll", StringComparison.OrdinalIgnoreCase ) ) )
                {
                    throw new AssertionFailedException( $"Cannot find {assemblyName}." );
                }
            }

            this.StandardCompileTimeMetadataReferences =
                this.SystemAssemblyPaths
                    .Concat( metalamaImplementationPaths )
                    .Select( MetadataReferenceCache.GetFromFile )
                    .Prepend( frameworkAssemblyReference )
                    .ToImmutableArray();
        }

        private IEnumerable<string> GetSystemAssemblyPaths()
        {
            var tempProjectDirectory = Path.Combine( this._cacheDirectory, nameof(ReferenceAssemblyLocator) );

            using var mutex = MutexHelper.WithGlobalLock( tempProjectDirectory, this._logger );
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

            GlobalJsonWriter.TryWriteCurrentVersion( tempProjectDirectory );

            var metadataReader = AssemblyMetadataReader.GetInstance( typeof(ReferenceAssemblyLocator).Assembly );

            // We don't add a reference to Microsoft.CSharp because this package is used to support dynamic code, and we don't want
            // dynamic code at compile time. We prefer compilation errors.
            var projectText =
                $@"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include='Microsoft.CodeAnalysis.CSharp' Version='{metadataReader.GetPackageVersion( "Microsoft.CodeAnalysis.CSharp" )}' />
    <PackageReference Include='System.Collections.Immutable' Version='{metadataReader.GetPackageVersion( "System.Collections.Immutable" )}' />
  </ItemGroup>
  <Target Name='WriteReferenceAssemblies' DependsOnTargets='FindReferenceAssembliesForReferences'>
    <WriteLinesToFile File='assemblies.txt' Overwrite='true' Lines='@(ReferencePathWithRefAssemblies)' />
  </Target>
</Project>";

            File.WriteAllText( Path.Combine( tempProjectDirectory, "TempProject.csproj" ), projectText );

            string dotnetPath;

            if ( RuntimeInformation.IsOSPlatform( OSPlatform.Windows ) )
            {
                dotnetPath = Environment.ExpandEnvironmentVariables( "%ProgramFiles%\\dotnet\\dotnet.exe" );

                if ( !File.Exists( dotnetPath ) )
                {
                    dotnetPath = Environment.ExpandEnvironmentVariables( "%ProgramFiles(x86)%\\dotnet\\dotnet.exe" );
                }

                if ( !File.Exists( dotnetPath ) )
                {
                    dotnetPath = "dotnet";
                }
            }
            else
            {
                dotnetPath = "dotnet";
            }

            // We may consider executing msbuild.exe instead of dotnet.exe when the build itself runs using msbuild.exe.
            // This way we wouldn't need to require a .NET SDK to be installed. Also, it seems that Rider requires the full path.
            // TODO 29508: Make this cross-platform.
            var psi = new ProcessStartInfo( dotnetPath, "build -t:WriteReferenceAssemblies" )
            {
                // We cannot call dotnet.exe with a \\?\-prefixed path because MSBuild would fail.
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