// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Maintenance;
using Metalama.Backstage.Utilities;
using Metalama.Compiler;
using Metalama.Framework.Engine.AspectWeavers;
using Metalama.Framework.Engine.Collections;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Metalama.Framework.Engine.CompileTime
{
    /// <summary>
    /// Provides the location to the reference assemblies that are needed to create the compile-time projects.
    /// This is achieved by creating an MSBuild project and restoring it.
    /// </summary>
    internal class ReferenceAssemblyLocator : IService
    {
        public const string TempDirectory = "AssemblyLocator";

        private const string _compileTimeFrameworkAssemblyName = "Metalama.Framework";
        private readonly string _cacheDirectory;
        private readonly ILogger _logger;
        private readonly string? _dotNetSdkDirectory;

        /// <summary>
        /// Gets the name (without path and extension) of Metalama assemblies.
        /// </summary>
        private ImmutableArray<string> MetalamaImplementationAssemblyNames { get; }

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
            this._cacheDirectory = serviceProvider.GetRequiredBackstageService<ITempFileManager>()
                .GetTempDirectory( TempDirectory, CleanUpStrategy.WhenUnused );

            this._logger = serviceProvider.GetLoggerFactory().GetLogger( nameof(ReferenceAssemblyLocator) );

            var platformInfo = (IPlatformInfo?) serviceProvider.GetService( typeof(IPlatformInfo) );

            if ( platformInfo != null )
            {
                this._dotNetSdkDirectory = platformInfo.DotNetSdkDirectory;
                this._logger.Trace?.Log( $"Platform information available. DotNetSdkDirectory = '{this._dotNetSdkDirectory}'." );
            }
            else
            {
                this._logger.Trace?.Log( $"Platform information not available." );
            }

            // Get Metalama implementation assemblies (but not the public API, for which we need a special compile-time build).
            var metalamaImplementationAssemblies =
                new[] { typeof(IAspectWeaver), typeof(TemplateSyntaxFactory) }.ToDictionary(
                    x => x.Assembly.GetName().Name,
                    x => x.Assembly.Location );

            // Force Metalama.Compiler.Interface to be loaded in the AppDomain.
            MetalamaCompilerInfo.EnsureInitialized();

            // Add the Metalama.Compiler.Interface" assembly. We cannot get it through typeof because types are directed to Microsoft.CodeAnalysis at compile time.
            // Strangely, there can be many instances of this same assembly.
            var metalamaCompilerInterfaceAssembly = AppDomainUtility
                .GetLoadedAssemblies( a => a.FullName.StartsWith( "Metalama.Compiler.Interface,", StringComparison.Ordinal ) )
                .OrderByDescending( a => a.GetName().Version )
                .FirstOrDefault();

            if ( metalamaCompilerInterfaceAssembly == null )
            {
                this._logger.Error?.Log( "Cannot find the Metalama.Compiler.Interface assembly in the AppDomain." );

                if ( this._logger.Trace != null )
                {
                    foreach ( var assembly in AppDomainUtility.GetLoadedAssemblies( _ => true ).OrderBy( a => a.ToString() ) )
                    {
                        this._logger.Trace.Log( "Loaded: " + assembly );
                    }
                }

                throw new AssertionFailedException( "Cannot find the Metalama.Compiler.Interface assembly." );
            }

            metalamaImplementationAssemblies.Add(
                "Metalama.Compiler.Interface",
                metalamaCompilerInterfaceAssembly.Location );

            this.MetalamaImplementationAssemblyNames = metalamaImplementationAssemblies.Keys.ToImmutableArray();
            var metalamaImplementationPaths = metalamaImplementationAssemblies.Values;

            // Get system assemblies.
            this.SystemAssemblyPaths = this.GetSystemAssemblyPaths().ToImmutableArray();

            this.SystemAssemblyNames = this.SystemAssemblyPaths
                .Select( Path.GetFileNameWithoutExtension )
                .ToImmutableHashSet( StringComparer.OrdinalIgnoreCase );

            // Sets the collection of all standard assemblies, i.e. system assemblies and ours.
            this.StandardAssemblyNames = this.MetalamaImplementationAssemblyNames
                .Concat( new[] { _compileTimeFrameworkAssemblyName } )
                .Concat( this.SystemAssemblyPaths.Select( Path.GetFileNameWithoutExtension ) )
                .ToImmutableHashSet( StringComparer.OrdinalIgnoreCase );

            // Also provide our embedded assemblies.

            var embeddedAssemblies =
                new[] { _compileTimeFrameworkAssemblyName, "Metalama.Compiler.Interface" }.Select(
                    name => (MetadataReference)
                        MetadataReference.CreateFromStream(
                            this.GetType().Assembly.GetManifestResourceStream( name + ".dll" )
                            ?? throw new InvalidOperationException( $"{name}.dll not found in assembly manifest resources." ),
                            filePath: $"[{this.GetType().Assembly.Location}]{name}.dll" ) );

            this.StandardCompileTimeMetadataReferences =
                this.SystemAssemblyPaths
                    .Concat( metalamaImplementationPaths )
                    .Select( MetadataReferenceCache.GetMetadataReference )
                    .Concat( embeddedAssemblies )
                    .ToImmutableArray();
        }

        private IEnumerable<string> GetSystemAssemblyPaths()
        {
            using var mutex = MutexHelper.WithGlobalLock( this._cacheDirectory, this._logger );
            var referenceAssemblyListFile = Path.Combine( this._cacheDirectory, "assemblies.txt" );

            if ( File.Exists( referenceAssemblyListFile ) )
            {
                var referenceAssemblies = File.ReadAllLines( referenceAssemblyListFile );

                if ( referenceAssemblies.All( File.Exists ) )
                {
                    return referenceAssemblies;
                }
            }

            Directory.CreateDirectory( this._cacheDirectory );

            GlobalJsonWriter.TryWriteCurrentVersion( this._cacheDirectory );

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

            File.WriteAllText( Path.Combine( this._cacheDirectory, "TempProject.csproj" ), projectText );

            var dotnetPath = PlatformUtilities.GetDotNetPath( this._logger, this._dotNetSdkDirectory );

            // We may consider executing msbuild.exe instead of dotnet.exe when the build itself runs using msbuild.exe.
            // This way we wouldn't need to require a .NET SDK to be installed. Also, it seems that Rider requires the full path.
            const string arguments = "build -t:WriteReferenceAssemblies";

            var psi = new ProcessStartInfo( dotnetPath, arguments )
            {
                // We cannot call dotnet.exe with a \\?\-prefixed path because MSBuild would fail.
                WorkingDirectory = this._cacheDirectory,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            var process = Process.Start( psi ).AssertNotNull();

            var lines = new List<string>();
            process.OutputDataReceived += ( _, e ) => lines.Add( e.Data );
            process.ErrorDataReceived += ( _, e ) => lines.Add( e.Data );

            process.BeginOutputReadLine();
            process.WaitForExit();

            if ( process.ExitCode != 0 )
            {
                throw new InvalidOperationException(
                    $"Error while building temporary project to locate reference assemblies: `{dotnetPath} {arguments}` returned {process.ExitCode}"
                    + Environment.NewLine + string.Join( Environment.NewLine, lines ) );
            }

            return File.ReadAllLines( referenceAssemblyListFile );
        }
    }
}