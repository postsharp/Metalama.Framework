// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Utilities;
using Caravela.Framework.Sdk;
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
        private static readonly string _projectText;

        private static readonly string _projectHash;

        private static ReferenceAssemblyLocator? _instance;

        static ReferenceAssemblyLocator()
        {
            AssemblyMetadataReader metadataReader = AssemblyMetadataReader.GetInstance( typeof(ReferenceAssemblyLocator).Assembly );
            
            _projectText = 
                $@"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include='Microsoft.CSharp' Version='{metadataReader.GetPackageVersion( "Microsoft.CSharp" )}' />
    <PackageReference Include='Microsoft.CodeAnalysis.CSharp' Version='{metadataReader.GetPackageVersion("Microsoft.CodeAnalysis.CSharp" )}' />
  </ItemGroup>
  <Target Name='WriteReferenceAssemblies' DependsOnTargets='FindReferenceAssembliesForReferences'>
    <WriteLinesToFile File='assemblies.txt' Overwrite='true' Lines='@(ReferencePathWithRefAssemblies)' />
  </Target>
</Project>";

            _projectHash = HashUtilities.HashString( _projectText );
        }

        public static ReferenceAssemblyLocator GetInstance()
        {
            // We don't initialize the instance from the static constructor because the constructor is non-trivial
            // and can fail, and it is difficult to debug an exception that occurs in a static constructor.
            _instance ??= new ReferenceAssemblyLocator();

            return _instance;
        }

        /// <summary>
        /// Gets the name (without path and extension) of Caravela assemblies.
        /// </summary>
        public ImmutableArray<string> CaravelaAssemblyNames { get; } = ImmutableArray.Create(
            "Caravela.Framework",
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
        public ImmutableArray<string> StandardAssemblyPaths { get; }

        private ReferenceAssemblyLocator()
        {
            this.SystemAssemblyPaths = GetSystemAssemblyPaths().ToImmutableArray();

            this.SystemAssemblyNames = this.SystemAssemblyPaths
                .Select( Path.GetFileNameWithoutExtension )
                .ToImmutableHashSet( StringComparer.OrdinalIgnoreCase );

            this.StandardAssemblyNames = this.CaravelaAssemblyNames
                .Concat( this.SystemAssemblyPaths )
                .ToImmutableHashSet( StringComparer.OrdinalIgnoreCase );

            // the SDK assembly might not be loaded at this point, so make sure it is
            _ = new AspectWeaverAttribute( null! );

            var caravelaPaths = AppDomain.CurrentDomain.GetAssemblies()
                .Where( a => !a.IsDynamic ) // accessing Location of dynamic assemblies throws
                .Select( a => a.Location )
                .Where( path => this.CaravelaAssemblyNames.Contains( Path.GetFileNameWithoutExtension( path ) ) );

            this.StandardAssemblyPaths = this.SystemAssemblyPaths.Concat( caravelaPaths ).ToImmutableArray();
        }

        private static IEnumerable<string> GetSystemAssemblyPaths()
        {
            var tempProjectDirectory = Path.Combine( Path.GetTempPath(), "Caravela", _projectHash, "TempProject" );

            using var mutex = MutexHelper.CreateGlobalMutex( tempProjectDirectory );
            mutex.WaitOne();

            try
            {
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

                File.WriteAllText( Path.Combine( tempProjectDirectory, "TempProject.csproj" ), _projectText );

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
            finally
            {
                mutex.ReleaseMutex();
            }
        }
    }
}