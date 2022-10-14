// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using K4os.Hash.xxHash;
using Metalama.Compiler;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.Collections;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Templating.Mapping;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Fabrics;
using Metalama.Framework.Project;
using Metalama.Framework.Validation;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Metalama.Framework.Engine.CompileTime
{
    /// <summary>
    /// Represents the compile-time project extracted from a run-time project, including its
    /// <see cref="System.Reflection.Assembly"/> allowing for execution, and metadata.
    /// </summary>
    public sealed class CompileTimeProject : IService
    {
        private static readonly Assembly _frameworkAssembly = typeof(IAspect).Assembly;
        private static readonly AssemblyIdentity _frameworkAssemblyIdentity = _frameworkAssembly.GetName().ToAssemblyIdentity();

        private static readonly CompileTimeProjectManifest _frameworkProjectManifest = new(
            _frameworkAssemblyIdentity.ToString(),
            _frameworkAssemblyIdentity.ToString(),
            "",
            new[] { typeof(InternalImplementAttribute) }
                .Select( t => t.FullName )
                .ToImmutableArray(),
            ImmutableArray<string>.Empty,
            ImmutableArray<string>.Empty,
            ImmutableArray<string>.Empty,
            ImmutableArray<string>.Empty,
            ImmutableArray<string>.Empty,
            null,
            0,
            ImmutableArray<CompileTimeFile>.Empty );

        private static DiagnosticManifest? _frameworkDiagnosticManifest;

        internal static CompileTimeProject CreateFrameworkProject( IServiceProvider serviceProvider, CompileTimeDomain domain )
        {
            var project = new CompileTimeProject(
                serviceProvider,
                domain,
                _frameworkAssemblyIdentity,
                _frameworkAssemblyIdentity,
                ImmutableArray<CompileTimeProject>.Empty,
                _frameworkProjectManifest,
                null,
                _ => null,
                null,
                _frameworkAssembly,
                _frameworkDiagnosticManifest );

            // Cache the diagnostic manifest for the next time.
            _frameworkDiagnosticManifest ??= project.DiagnosticManifest;

            return project;
        }

        private readonly CompileTimeProjectManifest? _manifest;
        private readonly string? _compiledAssemblyPath;
        private readonly AssemblyIdentity? _compileTimeIdentity;
        private readonly Func<string, TextMapFile?>? _getLocationMap;

        public CompileTimeDomain Domain { get; }

        internal DiagnosticManifest DiagnosticManifest { get; }

        private Assembly? _assembly;

        /// <summary>
        /// Gets the full path of the directory containing the transformed source code (typically under a temp directory). 
        /// This property is <c>null</c> is the current instance represents an empty project.
        /// </summary>
        public string? Directory { get; }

        /// <summary>
        /// Gets the identity of the run-time assembly for which this compile-time project was created.
        /// </summary>
        public AssemblyIdentity RunTimeIdentity { get; }

        /// <summary>
        /// Gets the list of aspect types (identified by their fully qualified reflection name) of the aspects
        /// declared in the current project.
        /// </summary>
        public IReadOnlyList<string> AspectTypes => this._manifest?.AspectTypes ?? Array.Empty<string>();

        public IReadOnlyList<string> OtherTemplateTypes => this._manifest?.OtherTemplateTypes ?? Array.Empty<string>();

        /// <summary>
        /// Gets the list of types that are exported using the <c>CompilerPlugin</c> attribute.
        /// </summary>
        public IReadOnlyList<string> PlugInTypes => this._manifest?.PlugInTypes ?? Array.Empty<string>();

        /// <summary>
        /// Gets the list of types that implement the <see cref="Fabric"/> interface, but the <see cref="TransitiveProjectFabric"/>.
        /// </summary>
        public IReadOnlyList<string> FabricTypes => this._manifest?.FabricTypes ?? Array.Empty<string>();

        /// <summary>
        /// Gets the list of types that implement the <see cref="TransitiveProjectFabric"/> interface.
        /// </summary>
        public IReadOnlyList<string> TransitiveFabricTypes => this._manifest?.TransitiveFabricTypes ?? Array.Empty<string>();

        /// <summary>
        /// Gets the list of compile-time projects referenced by the current project.
        /// </summary>
        public IReadOnlyList<CompileTimeProject> References { get; }

        public IReadOnlyList<CompileTimeProject> ClosureProjects { get; }

        /// <summary>
        /// Gets the list of transformed code files in the current project. 
        /// </summary>
        internal IReadOnlyList<CompileTimeFile> CodeFiles => this._manifest?.Files ?? Array.Empty<CompileTimeFile>();

        [Memo]
        internal ImmutableDictionaryOfArray<string, (CompileTimeFile File, CompileTimeProject Project)> ClosureCodeFiles
            => this.ClosureProjects.SelectMany( p => p.CodeFiles.Select( f => (f, p) ) ).ToMultiValueDictionary( f => f.f.TransformedPath, f => f );

        /// <summary>
        /// Gets a <see cref="MetadataReference"/> corresponding to the current project.
        /// </summary>
        /// <returns></returns>
        public MetadataReference ToMetadataReference() => MetadataReferenceCache.GetMetadataReference( this.AssertNotEmpty()._compiledAssemblyPath! );

        /// <summary>
        /// Gets a <see cref="CompileTime.ProjectLicenseInfo"/> corresponding to the current project.
        /// </summary>
        [Memo]
        public ProjectLicenseInfo ProjectLicenseInfo
            => this._manifest?.RedistributionLicenseKey == null
                ? ProjectLicenseInfo.Empty
                : new ProjectLicenseInfo( this._manifest.RedistributionLicenseKey );

        /// <summary>
        /// Gets the unique hash of the project, computed from the source code.
        /// </summary>
        public ulong Hash => this._manifest?.SourceHash ?? 0;

        /// <summary>
        /// Gets a value indicating whether the current project is empty, i.e. does not contain any source code. Note that
        /// an empty project can STILL contain <see cref="References"/>.
        /// </summary>
        public bool IsEmpty => this._compiledAssemblyPath == null && !this.IsFramework;

        public bool IsFramework => this.RunTimeIdentity.Name == "Metalama.Framework";

        /// <summary>
        /// Gets the CLR <see cref="System.Reflection.Assembly"/>, and loads it if necessary.
        /// </summary>
        private Assembly Assembly
        {
            get
            {
                this.LoadAssembly();

                return this._assembly!;
            }
        }

        private CompileTimeProject AssertNotEmpty()
        {
            if ( this.IsEmpty )
            {
                throw new InvalidOperationException();
            }

            return this;
        }

        private CompileTimeProject(
            IServiceProvider serviceProvider,
            CompileTimeDomain domain,
            AssemblyIdentity runTimeIdentity,
            AssemblyIdentity compileTimeIdentity,
            IReadOnlyList<CompileTimeProject> references,
            CompileTimeProjectManifest? manifest,
            string? compiledAssemblyPath,
            Func<string, TextMapFile?>? getLocationMap,
            string? directory,
            Assembly? assembly = null,
            DiagnosticManifest? diagnosticManifest = null )
        {
            this.Domain = domain;
            this._compiledAssemblyPath = compiledAssemblyPath;
            this._getLocationMap = getLocationMap;
            this.Directory = directory;
            this._manifest = manifest;
            this.RunTimeIdentity = runTimeIdentity;
            this._compileTimeIdentity = compileTimeIdentity;
            this.References = references;

            this._assembly = assembly;
            this.ClosureProjects = this.SelectManyRecursive( p => p.References, true, false ).ToImmutableList();
            this.DiagnosticManifest = diagnosticManifest ?? this.GetDiagnosticManifest( serviceProvider );
            this.ClosureDiagnosticManifest = new DiagnosticManifest( this.ClosureProjects.Select( p => p.DiagnosticManifest ).ToList() );

            // Check that the directory is valid.
            if ( manifest != null && directory != null )
            {
                foreach ( var file in manifest.Files )
                {
                    var path = Path.Combine( directory, file.TransformedPath );

                    if ( !File.Exists( path ) )
                    {
                        throw new InvalidOperationException(
                            $"'The directory '{directory}' is in invalid state. Terminate all build processes, delete the directory and retry the build." );
                    }
                }
            }
        }

        /// <summary>
        /// Creates a <see cref="CompileTimeProject"/> that includes source code.
        /// </summary>
        internal static CompileTimeProject Create(
            IServiceProvider serviceProvider,
            CompileTimeDomain domain,
            AssemblyIdentity runTimeIdentity,
            AssemblyIdentity compileTimeIdentity,
            IReadOnlyList<CompileTimeProject> references,
            CompileTimeProjectManifest manifest,
            string? compiledAssemblyPath,
            string? sourceDirectory,
            Func<string, TextMapFile?> getLocationMap )
            => new(
                serviceProvider,
                domain,
                runTimeIdentity,
                compileTimeIdentity,
                references,
                manifest,
                compiledAssemblyPath,
                getLocationMap,
                sourceDirectory );

        /// <summary>
        /// Creates a <see cref="CompileTimeProject"/> that does not include any source code.
        /// </summary>
        public static CompileTimeProject CreateEmpty(
            IServiceProvider serviceProvider,
            CompileTimeDomain domain,
            AssemblyIdentity runTimeIdentity,
            AssemblyIdentity compileTimeIdentity,
            IReadOnlyList<CompileTimeProject>? references = null )
            => new( serviceProvider, domain, runTimeIdentity, compileTimeIdentity, references ?? Array.Empty<CompileTimeProject>(), null, null, null, null );

        /// <summary>
        /// Creates a <see cref="CompileTimeProject"/> for an assembly that contains Metalama compile-time code but has not been transformed. This is the case
        /// normally for public APIs of SDK-based extensions. Returns <c>false</c> if the assembly is not loaded in the current AppDomain because it means
        /// it has not been loaded as an analyzer.
        /// </summary>
        public static bool TryCreateUntransformed(
            IServiceProvider serviceProvider,
            CompileTimeDomain domain,
            AssemblyIdentity assemblyIdentity,
            string assemblyPath,
            [NotNullWhen( true )] out CompileTimeProject? compileTimeProject )
        {
            var assemblyName = new AssemblyName( assemblyIdentity.ToString() );
            var assembly = AppDomainUtility.GetLoadedAssemblies( a => AssemblyName.ReferenceMatchesDefinition( assemblyName, a.GetName() ) ).FirstOrDefault();

            if ( assembly == null )
            {
                compileTimeProject = null;

                return false;
            }

            // Find interesting types.
            var aspectTypes = assembly.GetTypes().Where( t => typeof(IAspect).IsAssignableFrom( t ) ).Select( t => t.FullName ).ToImmutableArray();

            var fabricTypes = assembly.GetTypes()
                .Where( t => typeof(ProjectFabric).IsAssignableFrom( t ) && !typeof(TransitiveProjectFabric).IsAssignableFrom( t ) )
                .Select( t => t.FullName )
                .ToImmutableArray();

            var transitiveFabricTypes = assembly.GetTypes()
                .Where( t => typeof(TransitiveProjectFabric).IsAssignableFrom( t ) )
                .Select( t => t.FullName )
                .ToImmutableArray();

            var templateProviders =
                assembly.GetTypes().Where( t => typeof(ITemplateProvider).IsAssignableFrom( t ) ).Select( t => t.FullName ).ToImmutableArray();

            // Compute a unique hash based on the binary. 
            XXH64 hash = new();
            var buffer = new byte[1024];

            using ( var file = File.OpenRead( assemblyPath ) )
            {
                int read;

                while ( (read = file.Read( buffer, 0, 1024 )) > 0 )
                {
                    hash.Update( buffer, 0, read );
                }
            }

            // Create a manifest.
            var manifest = new CompileTimeProjectManifest(
                assemblyIdentity.ToString(),
                assemblyIdentity.ToString(),
                "",
                aspectTypes,
                Array.Empty<string>(),
                fabricTypes,
                transitiveFabricTypes,
                templateProviders,
                null,
                null,
                hash.Digest(),
                Array.Empty<CompileTimeFile>() );

            compileTimeProject = new CompileTimeProject(
                serviceProvider,
                domain,
                assemblyIdentity,
                assemblyIdentity,
                Array.Empty<CompileTimeProject>(),
                manifest,
                assemblyPath,
                null,
                null,
                assembly );

            return true;
        }

        /// <summary>
        /// Serializes the current project (its manifest and source code) into a stream that can be embedded as a managed resource.
        /// </summary>
        private void Serialize( Stream stream )
        {
            this.AssertNotEmpty();

            using ( var archive = new ZipArchive( stream, ZipArchiveMode.Create, true, Encoding.UTF8 ) )
            {
                // Write syntax trees.

                foreach ( var sourceFile in this.CodeFiles )
                {
                    var sourceText = File.ReadAllText( Path.Combine( this.Directory!, sourceFile.TransformedPath ) );

                    var entry = archive.CreateEntry( sourceFile.TransformedPath, CompressionLevel.Optimal );
                    using var entryWriter = new StreamWriter( entry.Open() );
                    entryWriter.Write( sourceText );
                }

                // Write manifest.
                var manifestEntry = archive.CreateEntry( "manifest.json", CompressionLevel.Optimal );
                var manifestStream = manifestEntry.Open();
                this._manifest!.Serialize( manifestStream );
            }
        }

        /// <summary>
        /// Returns a managed resource that contains the serialized project.
        /// </summary>
        /// <returns></returns>
        public ManagedResource ToResource()
        {
            this.AssertNotEmpty();

            var stream = new MemoryStream();
            this.Serialize( stream );
            var bytes = stream.ToArray();

            return new ManagedResource(
                CompileTimeConstants.CompileTimeProjectResourceName,
                bytes,
                true );
        }

        /// <summary>
        /// Gets a compile-time reflection <see cref="Type"/> defined in the current project.
        /// </summary>
        /// <param name="reflectionName"></param>
        /// <returns></returns>
        public Type? GetTypeOrNull( string reflectionName )
        {
            if ( this.IsEmpty )
            {
                return null;
            }
            else
            {
                var type = this.Assembly.GetType( reflectionName, false );

                if ( type == null )
                {
                    return null;
                }

                // Check that the type is linked properly. An error here may be caused by a bug in 
                // a handler of the AppDomain.AssemblyResolve event.
                var iAspectInterface = type.GetInterfaces().FirstOrDefault( i => i.FullName == typeof(IAspect).FullName );

                if ( iAspectInterface != null && !typeof(IAspect).IsAssignableFrom( iAspectInterface ) )
                {
                    // There must have been some assembly version mismatch.
                    throw new AssertionFailedException( "Assembly version mismatch." );
                }

                return type;
            }
        }

        public Type GetType( Type reflectionType ) => this.GetType( reflectionType.FullName! );

        public Type GetType( string reflectionName, string runTimeAssemblyName )
        {
            var project = this.ClosureProjects.FirstOrDefault( p => p.RunTimeIdentity.Name == runTimeAssemblyName );

            if ( project == null )
            {
                throw new InvalidOperationException( $"Cannot find the compile-time assembly 'P{runTimeAssemblyName}'." );
            }

            return project.GetType( reflectionName );
        }

        public Type GetType( string reflectionName )
            => this.GetTypeOrNull( reflectionName ) ?? throw new ArgumentOutOfRangeException(
                nameof(reflectionName),
                $"Cannot find a type named '{reflectionName}' in the compile-time project '{this._compileTimeIdentity}'." );

        internal (CompileTimeFile? File, CompileTimeProject? Project) FindCodeFileFromTransformedPath( string transformedCodePath )
        {
            return this.ClosureCodeFiles[Path.GetFileName( transformedCodePath )]
                .OrderByDescending( t => t.File.TransformedPath.Length )
                .FirstOrDefault();
        }

        private void LoadAssembly()
        {
            this.AssertNotEmpty();

            if ( this._assembly == null )
            {
                // We need to recursively load all dependent assemblies to prevent FileNotFoundException.

                foreach ( var reference in this.References )
                {
                    if ( !reference.IsEmpty )
                    {
                        reference.LoadAssembly();
                    }
                }

                this._assembly = this.Domain.GetOrLoadAssembly( this._compileTimeIdentity!, this._compiledAssemblyPath! );
            }
        }

        public override string ToString() => this.RunTimeIdentity.ToString();

        /// <summary>
        /// Gets a <see cref="TextMapFile"/> given a the path of the transformed code file.
        /// </summary>
        internal TextMapFile? GetTextMap( string csFilePath ) => this._getLocationMap?.Invoke( csFilePath );

        internal DiagnosticManifest ClosureDiagnosticManifest { get; }

        private DiagnosticManifest GetDiagnosticManifest( IServiceProvider serviceProvider )
        {
            var additionalTypes = new[] { typeof(FrameworkDiagnosticDescriptors) };

            var declaringTypes = Enumerable.Concat( this.AspectTypes.Concat( this.FabricTypes ), this.TransitiveFabricTypes )
                .Select( this.GetTypeOrNull )
                .Concat( additionalTypes )
                .WhereNotNull()
                .ToArray();

            var service = new DiagnosticDefinitionDiscoveryService( serviceProvider );
            var diagnostics = service.GetDiagnosticDefinitions( declaringTypes ).ToImmutableArray();
            var suppressions = service.GetSuppressionDefinitions( declaringTypes ).ToImmutableArray();

            return new DiagnosticManifest( diagnostics, suppressions );
        }
    }
}