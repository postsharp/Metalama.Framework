// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Maintenance;
using Metalama.Backstage.Utilities;
using Metalama.Compiler;
using Metalama.Framework.Engine.AspectWeavers;
using Metalama.Framework.Engine.Collections;
using Metalama.Framework.Engine.Options;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Metalama.Framework.Engine.Utilities.Threading;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
        private readonly ReferenceAssembliesManifest _referenceAssembliesManifest;
        private readonly IPlatformInfo _platformInfo;

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
            this._logger = serviceProvider.GetLoggerFactory().GetLogger( nameof(ReferenceAssemblyLocator) );

            this._platformInfo = serviceProvider.GetRequiredBackstageService<IPlatformInfo>();

            var projectOptions = serviceProvider.GetRequiredService<IProjectOptions>();

            string additionalPackageReferences;

            string additionalPackagesHash;

            if ( !projectOptions.CompileTimePackages.IsDefaultOrEmpty )
            {
                if ( string.IsNullOrEmpty( projectOptions.ProjectAssetsFile ) )
                {
                    throw new InvalidOperationException( "The CompileTimePackages property is defined, but ProjectAssetsFile is not." );
                }

                if ( string.IsNullOrEmpty( projectOptions.TargetFrameworkMoniker ) && string.IsNullOrWhiteSpace( projectOptions.TargetFramework ) )
                {
                    throw new InvalidOperationException(
                        "The CompileTimePackages property is defined, but both TargetFramework and TargetFrameworkMoniker are undefined." );
                }

                additionalPackageReferences = GetAdditionalPackageReferences( projectOptions );

                additionalPackagesHash = HashUtilities.HashString( additionalPackageReferences );
            }
            else
            {
                additionalPackageReferences = "";
                additionalPackagesHash = "default";
            }

            this._cacheDirectory = serviceProvider.GetRequiredBackstageService<ITempFileManager>()
                .GetTempDirectory( Path.Combine( TempDirectory, additionalPackagesHash ), CleanUpStrategy.WhenUnused );

            // Get Metalama implementation assemblies (but not the public API, for which we need a special compile-time build).
            var metalamaImplementationAssemblies =
                new[] { typeof(IAspectWeaver), typeof(TemplateSyntaxFactory) }.ToDictionary(
                    x => x.Assembly.GetName().Name.AssertNotNull(),
                    x => x.Assembly.Location );

            // Force Metalama.Compiler.Interface to be loaded in the AppDomain.
            MetalamaCompilerInfo.EnsureInitialized();

            // Add the Metalama.Compiler.Interface" assembly. We cannot get it through typeof because types are directed to Microsoft.CodeAnalysis at compile time.
            // Strangely, there can be many instances of this same assembly.

            // ReSharper disable once SimplifyLinqExpressionUseMinByAndMaxBy
            var metalamaCompilerInterfaceAssembly = AppDomainUtility
                .GetLoadedAssemblies( a => a.FullName != null! && a.FullName.StartsWith( "Metalama.Compiler.Interface,", StringComparison.Ordinal ) )
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
            this._referenceAssembliesManifest = this.GetReferenceAssembliesManifest( additionalPackageReferences );
            this.SystemAssemblyPaths = this._referenceAssembliesManifest.Assemblies;

            this.SystemAssemblyNames = this.SystemAssemblyPaths
                .Select( x => Path.GetFileNameWithoutExtension( x ).AssertNotNull() )
                .ToImmutableHashSet( StringComparer.OrdinalIgnoreCase );

            // Sets the collection of all standard assemblies, i.e. system assemblies and ours.
            this.StandardAssemblyNames = this.MetalamaImplementationAssemblyNames
                .Concat( new[] { _compileTimeFrameworkAssemblyName } )
                .Concat( this.SystemAssemblyPaths.Select( x => Path.GetFileNameWithoutExtension( x ).AssertNotNull() ) )
                .ToImmutableHashSet( StringComparer.OrdinalIgnoreCase );

            // Also provide our embedded assemblies.

            var embeddedAssemblies =
                new[] { _compileTimeFrameworkAssemblyName, "Metalama.Compiler.Interface" }.Select(
                    name => (MetadataReference)
                        MetadataReference.CreateFromStream(
                            this.GetType().Assembly.GetManifestResourceStream( name + ".dll" )
                            ?? throw new InvalidOperationException( $"{name}.dll not found in assembly manifest resources." ),
                            filePath: $"[{this.GetType().Assembly.Location}]{name}.dll" ) );

            this._logger.Trace?.Log( "System assemblies: " + string.Join( ", ", this.SystemAssemblyPaths ) );
            this._logger.Trace?.Log( "Metalama assemblies: " + string.Join( ", ", metalamaImplementationPaths ) );

            this.StandardCompileTimeMetadataReferences =
                this.SystemAssemblyPaths
                    .Concat( metalamaImplementationPaths )
                    .Select( MetadataReferenceCache.GetMetadataReference )
                    .Concat( embeddedAssemblies )
                    .ToImmutableArray();
        }

        private static string GetAdditionalPackageReferences( IProjectOptions options )
        {
            var resolvedPackages = new Dictionary<string, string>();

            var assetsJson = JObject.Parse( File.ReadAllText( options.ProjectAssetsFile.AssertNotNull() ) );
            JToken? packages = null;

            if ( !string.IsNullOrEmpty( options.TargetFrameworkMoniker ) )
            {
                packages = assetsJson["targets"]?[options.TargetFrameworkMoniker];
            }

            if ( packages == null && !string.IsNullOrEmpty( options.TargetFramework ) )
            {
                packages = assetsJson["targets"]?[options.TargetFramework];
            }

            if ( packages == null )
            {
                throw new InvalidOperationException(
                    $"'{options.ProjectAssetsFile}' does not contain targets for '{options.TargetFrameworkMoniker}' or '{options.TargetFramework}'." );
            }

            foreach ( var package in packages )
            {
                var nameVersion = ((JProperty) package).Name;
                var parts = nameVersion.Split( '/' );

                var packageName = parts[0];
                var packageVersion = parts[1];

                if ( options.CompileTimePackages.Contains( packageName ) )
                {
                    resolvedPackages.Add( packageName, $"\t\t<PackageReference Include=\"{packageName}\" Version=\"{packageVersion}\"/>" );
                }
            }

            var missingPackages = options.CompileTimePackages.Where( x => !resolvedPackages.ContainsKey( x ) ).ToList();

            if ( missingPackages.Count > 0 )
            {
                throw new InvalidOperationException(
                    $"No package was found for the following {MSBuildItemNames.MetalamaCompileTimePackage}: {string.Join( ", ", missingPackages )}" );
            }

            return string.Join( Environment.NewLine, resolvedPackages.OrderBy( x => x.Key ).Select( x => x.Value ) );
        }

        public bool IsSystemType( INamedTypeSymbol namedType )
        {
            var ns = namedType.ContainingNamespace.IsGlobalNamespace ? "" : namedType.ContainingNamespace.GetFullName().AssertNotNull();

            return this._referenceAssembliesManifest.Types.TryGetValue( ns, out var types ) && types.Contains( namedType.MetadataName );
        }

        private ReferenceAssembliesManifest GetReferenceAssembliesManifest( string additionalPackageReferences )
        {
            using ( MutexHelper.WithGlobalLock( this._cacheDirectory, this._logger ) )
            {
                var referencesJsonPath = Path.Combine( this._cacheDirectory, "references.json" );
                var assembliesListPath = Path.Combine( this._cacheDirectory, "assemblies.txt" );

                // See if the file is present in cache.
                if ( File.Exists( referencesJsonPath ) )
                {
                    this._logger.Trace?.Log( $"Reading '{referencesJsonPath}'." );

                    var referencesJson = File.ReadAllText( referencesJsonPath );

                    var manifest = JsonConvert.DeserializeObject<ReferenceAssembliesManifest>( referencesJson ).AssertNotNull();

                    var missingFiles = manifest.Assemblies.Where( f => !File.Exists( f ) ).ToList();

                    if ( missingFiles.Count == 0 )
                    {
                        return manifest;
                    }
                    else
                    {
                        this._logger.Warning?.Log(
                            $"The following file(s) did no longer exist so the reference project has to be rebuilt: {string.Join( ",", missingFiles )}." );
                    }
                }

                Directory.CreateDirectory( this._cacheDirectory );

                GlobalJsonWriter.WriteCurrentVersion( this._cacheDirectory, this._platformInfo );

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
{additionalPackageReferences}
  </ItemGroup>
  <Target Name='WriteReferenceAssemblies' DependsOnTargets='FindReferenceAssembliesForReferences'>
    <WriteLinesToFile File='{assembliesListPath}' Overwrite='true' Lines='@(ReferencePathWithRefAssemblies)' />
  </Target>
</Project>";

                var projectFilePath = Path.Combine( this._cacheDirectory, "TempProject.csproj" );
                this._logger.Trace?.Log( $"Writing '{projectFilePath}':" + Environment.NewLine + projectText );

                File.WriteAllText( projectFilePath, projectText );

                // Try to find the `dotnet` executable.

                // We may consider executing msbuild.exe instead of dotnet.exe when the build itself runs using msbuild.exe.
                // This way we wouldn't need to require a .NET SDK to be installed. Also, it seems that Rider requires the full path.
                const string arguments = "build -t:WriteReferenceAssemblies";
                var dotnetPath = this._platformInfo.DotNetExePath;

                var startInfo = new ProcessStartInfo( dotnetPath, arguments )
                {
                    // We cannot call dotnet.exe with a \\?\-prefixed path because MSBuild would fail.
                    WorkingDirectory = this._cacheDirectory,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    
                    // We must avoid passing the following environment variables to the child process, otherwise there can be a mismatch
                    // between SDK versions and the build will fail.
                    Environment = { { "DOTNET_ROOT_X64", null }, { "MSBUILD_EXE_PATH", null }, { "MSBuildSDKsPath", null } }
                };

                var process = new Process() { StartInfo = startInfo };

                var lines = new List<string>();

                void OnProcessDataReceived( object sender, DataReceivedEventArgs e )
                {
                    lines.Add( e.Data ?? "" );
                }

                process.OutputDataReceived += OnProcessDataReceived;
                process.ErrorDataReceived += OnProcessDataReceived;

                process.Start();
                process.BeginErrorReadLine();
                process.BeginOutputReadLine();
                process.WaitForExit();

                if ( process.ExitCode != 0 )
                {
                    throw new InvalidOperationException(
                        $"Error while building temporary project to locate reference assemblies: `\"{dotnetPath}\" {arguments}` in `{this._cacheDirectory}` returned {process.ExitCode}. Process output:"
                        + Environment.NewLine + Environment.NewLine + string.Join( Environment.NewLine, lines ) );
                }

                var assemblies = File.ReadAllLines( assembliesListPath );

                // Build the list of exported files.
                List<MetadataInfo> assemblyMetadatas = new();

                foreach ( var assemblyPath in assemblies )
                {
                    if ( !MetadataReader.TryGetMetadata( assemblyPath, out var metadataInfo ) )
                    {
                        throw new InvalidOperationException( $"Cannot read '{assemblyPath}'." );
                    }

                    assemblyMetadatas.Add( metadataInfo );
                }

                var exportedTypes = assemblyMetadatas
                    .SelectMany( m => m.ExportedTypes )
                    .GroupBy( ns => ns.Key )
                    .ToImmutableDictionary( ns => ns.Key, ns => ns.SelectMany( n => n.Value ).Distinct( StringComparer.Ordinal ).ToImmutableHashSet() );

                // Done.
                var result = new ReferenceAssembliesManifest( assemblies.ToImmutableArray(), exportedTypes );

                this._logger.Trace?.Log( $"Writing '{referencesJsonPath}'." );

                File.WriteAllText( referencesJsonPath, JsonConvert.SerializeObject( result, Newtonsoft.Json.Formatting.Indented ) );

                return result;
            }
        }
    }
}