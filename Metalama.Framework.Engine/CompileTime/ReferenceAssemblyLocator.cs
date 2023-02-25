// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Maintenance;
using Metalama.Compiler;
using Metalama.Framework.Aspects;
using Metalama.Framework.CompileTimeContracts;
using Metalama.Framework.Engine.AspectWeavers;
using Metalama.Framework.Engine.Options;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Metalama.Framework.Engine.Utilities.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

namespace Metalama.Framework.Engine.CompileTime
{
    /// <summary>
    /// Provides the location to the reference assemblies that are needed to create the compile-time projects.
    /// This is achieved by creating an MSBuild project and restoring it.
    /// </summary>
    internal sealed class ReferenceAssemblyLocator
    {
        private const string _compileTimeFrameworkAssemblyName = "Metalama.Framework";
        private readonly string _cacheDirectory;
        private readonly ILogger _logger;
        private readonly ReferenceAssembliesManifest _referenceAssembliesManifest;
        private readonly IPlatformInfo _platformInfo;
        private readonly DotNetTool _dotNetTool;

        /// <summary>
        /// Gets the name (without path and extension) of Metalama assemblies.
        /// </summary>
        private ImmutableArray<string> MetalamaImplementationAssemblyNames { get; }

        /// <summary>
        /// Gets the name (without path and extension) of all standard assemblies, including Metalama, Roslyn and .NET standard.
        /// </summary>
        public ImmutableHashSet<string> StandardAssemblyNames { get; }

        /// <summary>
        /// Gets the full path of reference system assemblies (.NET Standard and Roslyn). 
        /// </summary>
        public ImmutableArray<string> SystemReferenceAssemblyPaths { get; }

        /// <summary>
        /// Gets the full path of executable system assemblies for the current platform.
        /// </summary>
        public ImmutableArray<string> AdditionalCompileTimeAssemblyPaths { get; }

        public ImmutableDictionary<string, AssemblyIdentity> StandardAssemblyIdentities { get; }

        public bool IsStandardAssemblyName( string assemblyName )
            => string.Equals( assemblyName, "System.Private.CoreLib", StringComparison.OrdinalIgnoreCase )
               || this.StandardAssemblyNames.Contains( assemblyName );

        /// <summary>
        /// Gets the full path of all standard assemblies, including Metalama, Roslyn and .NET standard.
        /// </summary>
        public ImmutableArray<MetadataReference> StandardCompileTimeMetadataReferences { get; }

        public ReferenceAssemblyLocator( ProjectServiceProvider serviceProvider )
        {
            this._logger = serviceProvider.GetLoggerFactory().GetLogger( nameof(ReferenceAssemblyLocator) );

            this._platformInfo = serviceProvider.Global.GetRequiredBackstageService<IPlatformInfo>();
            this._dotNetTool = new DotNetTool( serviceProvider.Global );

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

            this._logger.Trace?.Log(
                "Assembly versions: " + string.Join(
                    ", ",
                    new[] { this.GetType(), typeof(IAspect), typeof(IAspectWeaver), typeof(ITemplateSyntaxFactory) }.SelectAsEnumerable(
                        x => x.Assembly.Location ) ) );

            this._cacheDirectory = serviceProvider.Global.GetRequiredBackstageService<ITempFileManager>()
                .GetTempDirectory( Path.Combine( TempDirectories.AssemblyLocator, additionalPackagesHash ), CleanUpStrategy.WhenUnused );

            // Get Metalama implementation contract assemblies (but not the public API, for which we need a special compile-time build).
            var metalamaImplementationAssemblies =
                new[] { typeof(IAspectWeaver), typeof(ITemplateSyntaxFactory) }.ToDictionary(
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
            this.SystemReferenceAssemblyPaths = this._referenceAssembliesManifest.ReferenceAssemblies;

            // Sets the collection of all standard assemblies, i.e. system assemblies and ours.
            this.StandardAssemblyNames = this.MetalamaImplementationAssemblyNames
                .Concat( new[] { _compileTimeFrameworkAssemblyName } )
                .Concat( this.SystemReferenceAssemblyPaths.Select( x => Path.GetFileNameWithoutExtension( x ).AssertNotNull() ) )
                .ToImmutableHashSet( StringComparer.OrdinalIgnoreCase );

            // Also provide our embedded assemblies.

            var embeddedAssemblies =
                new[] { _compileTimeFrameworkAssemblyName, "Metalama.Compiler.Interface" }.SelectAsImmutableArray(
                    name => (MetadataReference)
                        MetadataReference.CreateFromStream(
                            this.GetType().Assembly.GetManifestResourceStream( name + ".dll" )
                            ?? throw new InvalidOperationException( $"{name}.dll not found in assembly manifest resources." ),
                            filePath: $"[{this.GetType().Assembly.Location}]{name}.dll" ) );

            this._logger.Trace?.Log( "System assemblies: " + string.Join( ", ", this.SystemReferenceAssemblyPaths ) );
            this._logger.Trace?.Log( "Metalama assemblies: " + string.Join( ", ", metalamaImplementationPaths ) );

            this.StandardCompileTimeMetadataReferences =
                this.SystemReferenceAssemblyPaths
                    .Concat( metalamaImplementationPaths )
                    .Select( MetadataReferenceCache.GetMetadataReference )
                    .Concat( embeddedAssemblies )
                    .ToImmutableArray();

            var compilation = CSharpCompilation.Create( "ReferenceAssemblies", references: this.StandardCompileTimeMetadataReferences );
            this.StandardAssemblyIdentities = compilation.SourceModule.ReferencedAssemblySymbols.ToImmutableDictionary( s => s.Identity.Name, s => s.Identity );

            var platform = Environment.Version.Major < 6 ? "net471" : "net6.0";
            var binDirectory = Path.Combine( this._cacheDirectory, "bin", "Debug", platform );
            var files = Directory.GetFiles( binDirectory, "*.dll" );

            this.AdditionalCompileTimeAssemblyPaths =
                files.Where( p => !p.EndsWith( "TempProject.dll", StringComparison.OrdinalIgnoreCase ) ).ToImmutableArray();
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
                var assembliesListPath = Path.Combine( this._cacheDirectory, "assemblies-netstandard2.0.txt" );

                // See if the file is present in cache.
                if ( File.Exists( referencesJsonPath ) )
                {
                    this._logger.Trace?.Log( $"Reading '{referencesJsonPath}'." );

                    var referencesJson = File.ReadAllText( referencesJsonPath );

                    var manifest = JsonConvert.DeserializeObject<ReferenceAssembliesManifest>( referencesJson ).AssertNotNull();

                    var missingFiles = manifest.ReferenceAssemblies.Where( f => !File.Exists( f ) ).ToList();

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

                GlobalJsonHelper.WriteCurrentVersion( this._cacheDirectory, this._platformInfo );

                // We don't add a reference to Microsoft.CSharp because this package is used to support dynamic code, and we don't want
                // dynamic code at compile time. We prefer compilation errors.

                // We intentionally refer to the lowest supported Roslyn API version.
                // When we will support higher Roslyn features in templates, we will have to have reference assemblies for several versions.

                var projectText =
                    $"""
                        <Project Sdk="Microsoft.NET.Sdk">
                          <PropertyGroup>
                            <TargetFrameworks>netstandard2.0;net6.0;net471</TargetFrameworks>
                            <OutputType>Exe</OutputType>
                            <LangVersion>latest</LangVersion>
                          </PropertyGroup>
                          <ItemGroup>
                            <PackageReference Include="Microsoft.CodeAnalysis.CSharp" Version="4.0.1" />
                            <PackageReference Include="System.Collections.Immutable" Version="5.0.0" />
                            {additionalPackageReferences}
                          </ItemGroup>
                          <Target Name="WriteAssembliesList" AfterTargets="Build" Condition="'$(TargetFramework)'!=''">
                            <WriteLinesToFile File="assemblies-$(TargetFramework).txt" Overwrite="true" Lines="@(ReferencePathWithRefAssemblies)" />
                          </Target>
                        </Project>
                        """;

                var projectFilePath = Path.Combine( this._cacheDirectory, "TempProject.csproj" );
                this._logger.Trace?.Log( $"Writing '{projectFilePath}':" + Environment.NewLine + projectText );

                File.WriteAllText( projectFilePath, projectText );

                var programFilePath = Path.Combine( this._cacheDirectory, "Program.cs" );
                this._logger.Trace?.Log( $"Writing '{programFilePath}':" + Environment.NewLine + projectText );

                File.WriteAllText( programFilePath, "System.Console.WriteLine(\"Hello, world.\");" );

                // Try to find the `dotnet` executable.

                // We may consider executing msbuild.exe instead of dotnet.exe when the build itself runs using msbuild.exe.
                // This way we wouldn't need to require a .NET SDK to be installed. Also, it seems that Rider requires the full path.
                var arguments = $"build -bl:msbuild_{Guid.NewGuid().ToString().ReplaceOrdinal( "-", "" )}.binlog";

                this._dotNetTool.Execute( arguments, this._cacheDirectory );

                var assemblies = File.ReadAllLines( assembliesListPath );

                if ( assemblies.Length == 0 )
                {
                    throw new AssertionFailedException( $"The file '{assembliesListPath}' is empty." );
                }

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