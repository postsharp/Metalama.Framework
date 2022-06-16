// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using K4os.Hash.xxHash;
using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Utilities;
using Metalama.Compiler;
using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Formatting;
using Metalama.Framework.Engine.LamaSerialization;
using Metalama.Framework.Engine.Observers;
using Metalama.Framework.Engine.Options;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Templating.Mapping;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Fabrics;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Emit;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using System.Text;
using System.Threading;

namespace Metalama.Framework.Engine.CompileTime
{
    /// <summary>
    /// This class is responsible for building a compile-time <see cref="Compilation"/> based on a run-time one.
    /// </summary>
    internal partial class CompileTimeCompilationBuilder
    {
        private const string _compileTimeAssemblyPrefix = "MetalamaCompileTime_";

        private readonly IServiceProvider _serviceProvider;
        private readonly CompileTimeDomain _domain;
        private readonly Dictionary<ulong, CompileTimeProject> _cache = new();
        private readonly IPathOptions _pathOptions;
        private readonly IProjectOptions? _projectOptions;
        private readonly ICompileTimeCompilationBuilderObserver? _observer;
        private readonly ICompileTimeAssemblyBinaryRewriter? _rewriter;
        private readonly ILogger _logger;

        private static readonly Lazy<ImmutableDictionary<string,string>> _predefinedTypesSyntaxTree = new( GetPredefinedSyntaxTrees  );

        private static ImmutableDictionary<string,string> GetPredefinedSyntaxTrees()
        {
            var prefix = "Metalama.Framework.Engine._Resources_.";

            var assembly = typeof(CompileTimeCompilationBuilder).Assembly;

            return assembly.GetManifestResourceNames()
                .Where( n => n.StartsWith( prefix, StringComparison.Ordinal ) )
                .ToImmutableDictionary(
                    name => CompileTimeConstants.GetPrefixedSyntaxTreeName( name.Substring( prefix.Length ) )+ ".cs",
                    name =>
                    {
                        using var reader = new StreamReader( assembly.GetManifestResourceStream( name )! );

                        return reader.ReadToEnd();
                    } );
        }

        private static readonly Guid _buildId = AssemblyMetadataReader.GetInstance( typeof(CompileTimeCompilationBuilder).Assembly ).ModuleId;
        private readonly ReflectionMapperFactory _reflectionMapperFactory;
        private readonly SymbolClassificationService _classifierFactory;

        public CompileTimeCompilationBuilder( IServiceProvider serviceProvider, CompileTimeDomain domain )
        {
            this._pathOptions = serviceProvider.GetRequiredService<IPathOptions>();
            this._serviceProvider = serviceProvider;
            this._domain = domain;
            this._observer = serviceProvider.GetService<ICompileTimeCompilationBuilderObserver>();
            this._rewriter = serviceProvider.GetService<ICompileTimeAssemblyBinaryRewriter>();
            this._projectOptions = serviceProvider.GetService<IProjectOptions>();
            this._reflectionMapperFactory = serviceProvider.GetRequiredService<ReflectionMapperFactory>();
            this._classifierFactory = serviceProvider.GetRequiredService<SymbolClassificationService>();
            this._logger = serviceProvider.GetLoggerFactory().CompileTime();
        }

        private static ulong ComputeSourceHash( FrameworkName? targetFramework, IReadOnlyList<SyntaxTree> compileTimeTrees, StringBuilder? log = null )
        {
            if ( compileTimeTrees.Count == 0 )
            {
                return 0;
            }

            log?.AppendLine( nameof(ComputeSourceHash) );
            XXH64 h = new();

            // Hash the target framework.
            if ( targetFramework != null )
            {
                h.Update( targetFramework.FullName );
            }

            // Hash compilation symbols.
            var preprocessorSymbols = compileTimeTrees.Select( x => x.Options ).SelectMany( x => x.PreprocessorSymbolNames ).Distinct().OrderBy( x => x );

            foreach ( var symbol in preprocessorSymbols )
            {
                h.Update( symbol );
            }

            // Hash
            foreach ( var syntaxTree in compileTimeTrees.OrderBy( t => t.FilePath ) )
            {
                // SourceText.Checksum does not seem to return the same thing at compile time than at run time, so we take use our own algorithm.
                var text = syntaxTree.GetText().ToString();
                h.Update( text );

                log?.AppendLineInvariant( $"Source:{syntaxTree.FilePath}={string.Join( "", HashUtilities.HashString( text ) )}" );
            }

            var digest = h.Digest();
            log?.AppendLineInvariant( $"Digest:{digest:x}" );

            return digest;
        }

        private static ulong ComputeProjectHash(
            IEnumerable<CompileTimeProject> referencedProjects,
            ulong sourceHash,
            StringBuilder? log = null )
        {
            log?.AppendLine( nameof(ComputeProjectHash) );

            XXH64 h = new();
            h.Update( _buildId );
            log?.AppendLineInvariant( $"BuildId={_buildId}" );

            foreach ( var reference in referencedProjects.OrderBy( r => r.Hash ) )
            {
                h.Update( reference.Hash );
                log?.AppendLineInvariant( $"Reference:={reference.RunTimeIdentity.Name}={reference.Hash}" );
            }

            h.Update( sourceHash );
            log?.AppendLineInvariant( $"Source:={sourceHash:x}" );

            var digest = h.Digest();
            log?.AppendLineInvariant( $"Digest:{digest:x}" );

            return digest;
        }

        private bool TryCreateCompileTimeCompilation(
            Compilation runTimeCompilation,
            IReadOnlyList<SyntaxTree> treesWithCompileTimeCode,
            IEnumerable<CompileTimeProject> referencedProjects,
            ImmutableArray<UsingDirectiveSyntax> globalUsings,
            OutputPaths outputPaths,
            IDiagnosticAdder diagnosticSink,
            CancellationToken cancellationToken,
            out Compilation? compileTimeCompilation,
            out ILocationAnnotationMap? locationAnnotationMap )
        {
            locationAnnotationMap = null;

            // If there is no compile-time tree, there is no need to do anything.
            if ( treesWithCompileTimeCode.Count == 0 )
            {
                compileTimeCompilation = null;

                return true;
            }

            compileTimeCompilation = this.CreateEmptyCompileTimeCompilation( outputPaths.CompileTimeAssemblyName, referencedProjects );
            var serializableTypes = this.GetSerializableTypes( runTimeCompilation, treesWithCompileTimeCode, cancellationToken );

            var templateCompiler = new TemplateCompiler( this._serviceProvider, runTimeCompilation );

            var produceCompileTimeCodeRewriter = new ProduceCompileTimeCodeRewriter(
                runTimeCompilation,
                compileTimeCompilation,
                serializableTypes,
                globalUsings,
                diagnosticSink,
                templateCompiler,
                this._serviceProvider,
                cancellationToken );

            // Creates the new syntax trees. Store them in a dictionary mapping the transformed trees to the source trees.
            var syntaxTrees = treesWithCompileTimeCode.Select(
                    t => (TransformedTree: CSharpSyntaxTree.Create(
                                  (CSharpSyntaxNode) produceCompileTimeCodeRewriter.Visit( t.GetRoot() ).AssertNotNull(),
                                  CSharpParseOptions.Default,
                                  t.FilePath,
                                  Encoding.UTF8 )
                              .WithFilePath( GetTransformedFilePath( outputPaths, t.FilePath ) ),
                          SourceTree: t) )
                .ToList();

            locationAnnotationMap = templateCompiler.LocationAnnotationMap;

            if ( !produceCompileTimeCodeRewriter.Success )
            {
                return false;
            }

            if ( !produceCompileTimeCodeRewriter.FoundCompileTimeCode )
            {
                // This happens if all compile-time code is illegitimate, i.e. was reported as an error and stripped.

                compileTimeCompilation = null;

                return true;
            }

            compileTimeCompilation = compileTimeCompilation.AddSyntaxTrees( syntaxTrees.Select( t => t.TransformedTree ) );
            compileTimeCompilation = new RemoveInvalidUsingRewriter( compileTimeCompilation ).VisitTrees( compileTimeCompilation );

            if ( this._projectOptions is { FormatCompileTimeCode: true } && OutputCodeFormatter.CanFormat )
            {
                var formattedCompilation = OutputCodeFormatter.FormatAll( compileTimeCompilation );

                if ( !(formattedCompilation.GetDiagnostics().Any( d => d.Severity == DiagnosticSeverity.Error ) &&
                       !compileTimeCompilation.GetDiagnostics().Any( d => d.Severity == DiagnosticSeverity.Error )) )
                {
                    compileTimeCompilation = formattedCompilation;
                }
                else
                {
                    this._logger.Warning?.Log(
                        $"The formatting of the compile-time project '{compileTimeCompilation.AssemblyName}' failed. Falling back to the unformatted code." );
                }
            }

            this._observer?.OnCompileTimeCompilation( compileTimeCompilation );

            return true;
        }

        private static string GetTransformedFilePath( OutputPaths outputPaths, string originalFilePath )
        {
            // Find a decent and unique name.
            var transformedFileName = !string.IsNullOrWhiteSpace( originalFilePath )
                ? Path.GetFileNameWithoutExtension( originalFilePath )
                : "Anonymous";

            // Shorten the path if we may exceed the largest allowed size.
            var remainingSizeForName = 254 - outputPaths.Directory.Length - 1 /* backslash */ - 4 /* .xxx */ - 1 /* _ */ - 8 /* hash */;

            if ( transformedFileName.Length > remainingSizeForName )
            {
                transformedFileName = transformedFileName.Substring( 0, remainingSizeForName );
            }

            transformedFileName += "_" + HashUtilities.HashString( originalFilePath );
            transformedFileName += Path.GetExtension( originalFilePath );

            return transformedFileName;
        }

        public static bool TryParseCompileTimeAssemblyName( string assemblyName, [NotNullWhen( true )] out string? runTimeAssemblyName )
        {
            if ( assemblyName.StartsWith( _compileTimeAssemblyPrefix, StringComparison.OrdinalIgnoreCase ) )
            {
                var parsedAssemblyName = new AssemblyName( assemblyName );
                var shortName = parsedAssemblyName.Name;

                runTimeAssemblyName = shortName.Substring(
                    _compileTimeAssemblyPrefix.Length,
                    shortName.Length - _compileTimeAssemblyPrefix.Length - 17 );

                return true;
            }
            else
            {
                runTimeAssemblyName = null;

                return false;
            }
        }

        private CSharpCompilation CreateEmptyCompileTimeCompilation( string assemblyName, IEnumerable<CompileTimeProject> referencedProjects )
        {
            var assemblyLocator = this._serviceProvider.GetRequiredService<ReferenceAssemblyLocator>();

            var parseOptions = new CSharpParseOptions( preprocessorSymbols: new[] { "NETSTANDARD_2_0" } );

            var standardReferences = assemblyLocator.StandardCompileTimeMetadataReferences;

            var predefinedSyntaxTrees =
                _predefinedTypesSyntaxTree.Value.Select( x => CSharpSyntaxTree.ParseText( x.Value, parseOptions, x.Key, Encoding.UTF8 ) );

            return CSharpCompilation.Create(
                    assemblyName,
                    predefinedSyntaxTrees,
                    standardReferences,
                    new CSharpCompilationOptions( OutputKind.DynamicallyLinkedLibrary, deterministic: true ) )
                .AddReferences(
                    referencedProjects
                        .Where( r => !r.IsEmpty && !r.IsFramework )
                        .Select( r => r.ToMetadataReference() ) );
        }

        private bool TryEmit(
            OutputPaths outputPaths,
            Compilation compileTimeCompilation,
            IDiagnosticAdder diagnosticSink,
            TextMapDirectory? textMapDirectory,
            CancellationToken cancellationToken )
        {
            this._logger.Trace?.Log( $"TryEmit( '{compileTimeCompilation.AssemblyName}' )" );

            var outputDirectory = outputPaths.Directory.AssertNotNull();

            try
            {
                var emitOptions = new EmitOptions( debugInformationFormat: DebugInformationFormat.PortablePdb );

                // Write the generated files to disk if we should.
                if ( !Directory.Exists( outputDirectory ) )
                {
                    this._logger.Trace?.Log( $"Creating directory '{outputDirectory}'." );
                    Directory.CreateDirectory( outputDirectory );
                }

                compileTimeCompilation =
                    compileTimeCompilation.WithOptions( compileTimeCompilation.Options.WithOptimizationLevel( OptimizationLevel.Debug ) );

                foreach ( var compileTimeSyntaxTree in compileTimeCompilation.SyntaxTrees )
                {
                    var path = Path.Combine( outputDirectory, compileTimeSyntaxTree.FilePath );

                    if ( path.Length > 254 )
                    {
                        // We should generate, upstream, a path that is short enough. At this stage, it is too late to shorten it.
                        throw new AssertionFailedException( $"Path too long: '{path}'" );
                    }

                    var text = compileTimeSyntaxTree.GetText();

                    this._logger.Trace?.Log( $"Writing code to '{path}'." );

                    // Write the file in a retry loop to handle locks. It seems there are still file lock issues
                    // despite the Mutex. 
                    RetryHelper.RetryWithLockDetection(
                        path,
                        _ =>
                        {
                            using ( var textWriter = new StreamWriter( path, false, Encoding.UTF8 ) )
                            {
                                text.Write( textWriter, cancellationToken );
                            }
                        },
                        this._serviceProvider,
                        logger: this._logger );

                    // Update the link to the file path.
                    var newTree = CSharpSyntaxTree.Create(
                        (CSharpSyntaxNode) compileTimeSyntaxTree.GetRoot(),
                        (CSharpParseOptions?) compileTimeSyntaxTree.Options,
                        path,
                        Encoding.UTF8 );

                    compileTimeCompilation = compileTimeCompilation.ReplaceSyntaxTree( compileTimeSyntaxTree, newTree );
                }

                this._logger.Trace?.Log( $"Writing binary to '{outputPaths.Pe}'." );

                EmitResult? emitResult = null;

                if ( this._rewriter != null )
                {
                    // TryMetalama defines a binary rewriter to inject Unbreakable.

                    MemoryStream memoryStream = new();
                    emitResult = compileTimeCompilation.Emit( memoryStream, null, options: emitOptions, cancellationToken: cancellationToken );

                    if ( emitResult.Success )
                    {
                        memoryStream.Seek( 0, SeekOrigin.Begin );

                        using ( var peStream = File.Create( outputPaths.Pe ) )
                        {
                            this._rewriter.Rewrite( memoryStream, peStream, outputPaths.Pe );
                        }
                    }
                }
                else
                {
                    RetryHelper.RetryWithLockDetection(
                        outputPaths.Pe,
                        _ =>
                        {
                            using ( var peStream = File.Create( outputPaths.Pe ) )
                            using ( var pdbStream = File.Create( outputPaths.Pdb ) )
                            {
                                emitResult = compileTimeCompilation.Emit( peStream, pdbStream, options: emitOptions, cancellationToken: cancellationToken );
                            }
                        },
                        this._serviceProvider,
                        logger: this._logger );
                }

                this._observer?.OnCompileTimeCompilationEmit( compileTimeCompilation, emitResult!.Diagnostics );

                // Reports a diagnostic in the original syntax tree.
                void ReportDiagnostics( IEnumerable<Diagnostic> diagnostics )
                {
                    foreach ( var diagnostic in diagnostics )
                    {
                        textMapDirectory ??= TextMapDirectory.Load( outputDirectory );

                        var transformedPath = diagnostic.Location.SourceTree?.FilePath;

                        if ( !string.IsNullOrEmpty( transformedPath ) && textMapDirectory.TryGetByName( transformedPath!, out var mapFile ) )
                        {
                            var location = mapFile.GetSourceLocation( diagnostic.Location.SourceSpan );

                            var relocatedDiagnostic = Diagnostic.Create(
                                diagnostic.Id,
                                diagnostic.Descriptor.Category,
                                new NonLocalizedString( diagnostic.GetMessage() ),
                                diagnostic.Severity,
                                diagnostic.DefaultSeverity,
                                true,
                                diagnostic.WarningLevel,
                                location: location );

                            diagnosticSink.Report( relocatedDiagnostic );
                        }
                        else
                        {
                            // Coverage: ignore
                            // (this should happen only in case of incorrect generation of compile-time, but in this case a graceful fallback
                            // is better than an exception).

                            diagnosticSink.Report( diagnostic );
                        }
                    }
                }

                if ( !emitResult!.Success )
                {
                    // When the compile-time assembly is invalid, to enable troubleshooting, we store the source files and the list of diagnostics
                    // to a directory that will not be deleted after the build.
                    var troubleshootingDirectory = Path.Combine(
                        TempPathHelper.GetTempPath( "CompileTimeTroubleshooting" ),
                        Guid.NewGuid().ToString() );

                    Directory.CreateDirectory( troubleshootingDirectory );

                    foreach ( var syntaxTree in compileTimeCompilation.SyntaxTrees )
                    {
                        var path = Path.Combine( troubleshootingDirectory, Path.GetFileName( syntaxTree.FilePath ) );

                        using ( var writer = File.CreateText( path ) )
                        {
                            syntaxTree.GetText().Write( writer, cancellationToken );
                        }
                    }

                    var diagnosticPath = Path.Combine( troubleshootingDirectory, "errors.txt" );

                    using ( var errorFile = File.CreateText( diagnosticPath ) )
                    {
                        errorFile.WriteLine( "Diagnostics:" );

                        foreach ( var diagnostic in emitResult.Diagnostics )
                        {
                            errorFile.WriteLine( "  " + diagnostic );
                        }

                        errorFile.WriteLine( "References:" );

                        foreach ( var reference in compileTimeCompilation.References )
                        {
                            errorFile.WriteLine( "  " + reference.Display );
                        }
                    }

                    this._logger.Trace?.Log(
                        $"TryEmit( '{compileTimeCompilation.AssemblyName}' ): failure: " +
                        string.Join( Environment.NewLine, emitResult.Diagnostics ) );

                    diagnosticSink.Report(
                        TemplatingDiagnosticDescriptors.CannotEmitCompileTimeAssembly.CreateRoslynDiagnostic(
                            null,
                            troubleshootingDirectory ) );

                    ReportDiagnostics( emitResult.Diagnostics.Where( d => d.Severity >= DiagnosticSeverity.Error ) );

                    DeleteOutputFiles();

                    return false;
                }
                else
                {
                    this._logger.Trace?.Log( $"TryEmit( '{compileTimeCompilation.AssemblyName}' ): success." );

                    return true;
                }
            }
            catch ( Exception e )
            {
                this._logger.Trace?.Log( e.ToString() );

                DeleteOutputFiles();

                throw;
            }

            void DeleteOutputFiles()
            {
                this._logger.Warning?.Log( $"Deleting directory '{outputDirectory}'." );

                try
                {
                    // Try to delete with lock detection first to get a better error message.
                    if ( Directory.Exists( outputDirectory ) )
                    {
                        var files = Directory.GetFiles( outputDirectory );
                        RetryHelper.RetryWithLockDetection( files, File.Delete, this._serviceProvider );
                    }

                    // Then delete the directory itself. At this point, we should no longer have locks. 
                    RetryHelper.Retry(
                        () =>
                        {
                            if ( Directory.Exists( outputDirectory ) )
                            {
                                Directory.Delete( outputDirectory, true );
                            }
                        },
                        logger: this._logger );
                }
                catch ( Exception e )
                {
                    this._logger.Error?.Log( $"Cannot delete directory '{outputDirectory}': {e.Message}" );
                }
            }
        }

        private static List<UsingDirectiveSyntax> GetUsingsFromOptions( Compilation compilation )
        {
            return ((CSharpCompilation) compilation).Options.Usings.Select( x => SyntaxFactory.UsingDirective( ParseNamespace( x ) ).NormalizeWhitespace() )
                .ToList();

            static NameSyntax ParseNamespace( string ns )
            {
                var parts = ns.Split( '.' );
                NameSyntax result = SyntaxFactory.IdentifierName( parts[0] );

                for ( var i = 1; i < parts.Length; i++ )
                {
                    result = SyntaxFactory.QualifiedName( result, SyntaxFactory.IdentifierName( parts[i] ) );
                }

                return result;
            }
        }

        private (IReadOnlyList<SyntaxTree> SyntaxTrees, ImmutableArray<UsingDirectiveSyntax> GlobalUsings) GetCompileTimeArtifacts(
            Compilation runTimeCompilation,
            IReadOnlyList<SyntaxTree>? compileTimeTreesHint,
            CancellationToken cancellationToken )
        {
            List<SyntaxTree> compileTimeTrees = new();
            var globalUsings = GetUsingsFromOptions( runTimeCompilation );
            var classifier = this._serviceProvider.GetRequiredService<SymbolClassificationService>().GetClassifier( runTimeCompilation );

            var trees = compileTimeTreesHint ?? runTimeCompilation.SyntaxTrees;

            foreach ( var tree in trees )
            {
                FindCompileTimeCodeVisitor visitor = new( runTimeCompilation.GetSemanticModel( tree, true ), classifier, cancellationToken );
                visitor.Visit( tree.GetRoot() );

                if ( visitor.HasCompileTimeCode )
                {
                    compileTimeTrees.Add( tree );
                }

                globalUsings.AddRange( visitor.GlobalUsings.Select( syntax => SyntaxFactory.UsingDirective( syntax ).NormalizeWhitespace() ) );
            }

            return (compileTimeTrees, globalUsings.ToImmutableArray());
        }

        private IReadOnlyList<SerializableTypeInfo> GetSerializableTypes(
            Compilation runTimeCompilation,
            IReadOnlyList<SyntaxTree> compileTimeSyntaxTrees,
            CancellationToken cancellationToken )
        {
            // TODO: Check that the mapper is not already registered.
            var allSerializableTypes = new List<SerializableTypeInfo>();
            var reflectionMapper = this._reflectionMapperFactory.GetInstance( runTimeCompilation );
            var classifier = this._classifierFactory.GetClassifier( runTimeCompilation );

            foreach ( var tree in compileTimeSyntaxTrees )
            {
                var visitor = new CollectSerializableTypesVisitor(
                    runTimeCompilation.GetSemanticModel( tree, true ),
                    reflectionMapper,
                    classifier,
                    cancellationToken );

                visitor.Visit( tree.GetRoot() );

                allSerializableTypes.AddRange( visitor.SerializableTypes );
            }

            return allSerializableTypes;
        }

        /// <summary>
        /// Tries to create a compile-time <see cref="Compilation"/> given a run-time <see cref="Compilation"/>.
        /// </summary>
        internal bool TryGetCompileTimeProject(
            Compilation runTimeCompilation,
            IReadOnlyList<SyntaxTree>? compileTimeTreesHint,
            IReadOnlyList<CompileTimeProject> referencedProjects,
            IDiagnosticAdder diagnosticSink,
            bool cacheOnly,
            CancellationToken cancellationToken,
            out CompileTimeProject? project )
        {
            var compileTimeArtifacts = this.GetCompileTimeArtifacts( runTimeCompilation, compileTimeTreesHint, cancellationToken );

            return this.TryGetCompileTimeProjectImpl(
                runTimeCompilation,
                compileTimeArtifacts.SyntaxTrees,
                referencedProjects,
                compileTimeArtifacts.GlobalUsings,
                diagnosticSink,
                cacheOnly,
                cancellationToken,
                out project );
        }

        private bool TryGetCompileTimeProjectFromCache(
            Compilation runTimeCompilation,
            IReadOnlyList<CompileTimeProject> referencedProjects,
            OutputPaths outputPaths,
            ulong projectHash,
            out CompileTimeProject? project )
        {
            this._logger.Trace?.Log( $"TryGetCompileTimeProjectFromCache( '{runTimeCompilation.AssemblyName}' )" );

            // Look in in-memory cache.
            if ( this._cache.TryGetValue( projectHash, out project ) )
            {
                this._logger.Trace?.Log( $"TryGetCompileTimeProjectFromCache( '{runTimeCompilation.AssemblyName}' ): found in memory cache." );

                return true;
            }

            // Look on disk.
            if ( !File.Exists( outputPaths.Pe ) )
            {
                this._logger.Trace?.Log( $"TryGetCompileTimeProjectFromCache( '{runTimeCompilation.AssemblyName}' ): '{outputPaths.Pe}' not found." );

                project = null;

                return false;
            }

            if ( !File.Exists( outputPaths.Manifest ) )
            {
                this._logger.Trace?.Log( $"TryGetCompileTimeProjectFromCache( '{runTimeCompilation.AssemblyName}' ): '{outputPaths.Manifest}' not found." );

                project = null;

                return false;
            }

            this._logger.Trace?.Log( $"TryGetCompileTimeProjectFromCache( '{runTimeCompilation.AssemblyName}' ): found on disk. Deserializing." );

            // Deserialize the manifest.
            var manifest = CompileTimeProjectManifest.Deserialize( RetryHelper.Retry( () => File.OpenRead( outputPaths.Manifest ), logger: this._logger ) );

            project = CompileTimeProject.Create(
                this._serviceProvider,
                this._domain,
                runTimeCompilation.Assembly.Identity,
                new AssemblyIdentity( outputPaths.CompileTimeAssemblyName ),
                referencedProjects,
                manifest,
                outputPaths.Pe,
                outputPaths.Directory,
                TextMapFile.ReadForSource );

            this._cache.Add( projectHash, project );

            return true;
        }

        private bool TryGetCompileTimeProjectImpl(
            Compilation runTimeCompilation,
            IReadOnlyList<SyntaxTree> sourceTreesWithCompileTimeCode,
            IReadOnlyList<CompileTimeProject> referencedProjects,
            ImmutableArray<UsingDirectiveSyntax> globalUsings,
            IDiagnosticAdder diagnosticSink,
            bool cacheOnly,
            CancellationToken cancellationToken,
            out CompileTimeProject? project )
        {
            // Check the in-process cache.
            var (sourceHash, projectHash, outputPaths) =
                this.GetPreCacheProjectInfo( runTimeCompilation, sourceTreesWithCompileTimeCode, referencedProjects );

            if ( !this.TryGetCompileTimeProjectFromCache(
                    runTimeCompilation,
                    referencedProjects,
                    outputPaths,
                    projectHash,
                    out project ) )
            {
                if ( cacheOnly )
                {
                    // We were asked to get cache projects only. Don't create it.
                    project = null;

                    return false;
                }

                using ( this.WithLock( outputPaths.CompileTimeAssemblyName ) )
                {
                    // Do a second cache lookup within the lock.
                    if ( this.TryGetCompileTimeProjectFromCache(
                            runTimeCompilation,
                            referencedProjects,
                            outputPaths,
                            projectHash,
                            out project ) )
                    {
                        // Coverage: ignore (this depends on a multi-threaded condition)
                        return true;
                    }

                    // Generate the C# compilation.
                    if ( !this.TryCreateCompileTimeCompilation(
                            runTimeCompilation,
                            sourceTreesWithCompileTimeCode,
                            referencedProjects,
                            globalUsings,
                            outputPaths,
                            diagnosticSink,
                            cancellationToken,
                            out var compileTimeCompilation,
                            out var locationAnnotationMap ) )
                    {
                        project = null;

                        return false;
                    }

                    if ( compileTimeCompilation == null )
                    {
                        // The run-time compilation does not contain compile-time classes, but it can have compile-time references.

                        if ( referencedProjects.Count == 0 )
                        {
                            project = null;
                        }
                        else
                        {
                            project = CompileTimeProject.CreateEmpty(
                                this._serviceProvider,
                                this._domain,
                                runTimeCompilation.Assembly.Identity,
                                new AssemblyIdentity( outputPaths.CompileTimeAssemblyName ),
                                referencedProjects );
                        }

                        return true;
                    }
                    else
                    {
                        var textMapDirectory = TextMapDirectory.Create( compileTimeCompilation, locationAnnotationMap! );

                        if ( !this.TryEmit( outputPaths, compileTimeCompilation, diagnosticSink, textMapDirectory, cancellationToken ) )
                        {
                            project = null;

                            return false;
                        }

                        textMapDirectory.Write( outputPaths.Directory! );

                        var aspectType = compileTimeCompilation.GetTypeByMetadataName( typeof(IAspect).FullName );
                        var fabricType = compileTimeCompilation.GetTypeByMetadataName( typeof(Fabric).FullName );
                        var transitiveFabricType = compileTimeCompilation.GetTypeByMetadataName( typeof(TransitiveProjectFabric).FullName );
                        var templateProviderType = compileTimeCompilation.GetTypeByMetadataName( typeof(ITemplateProvider).FullName );

                        var aspectTypes = compileTimeCompilation.Assembly.GetTypes()
                            .Where( t => compileTimeCompilation.HasImplicitConversion( t, aspectType ) )
                            .Select( t => t.GetReflectionName().AssertNotNull() )
                            .ToList();

                        var fabricTypes = compileTimeCompilation.Assembly.GetTypes()
                            .Where(
                                t => compileTimeCompilation.HasImplicitConversion( t, fabricType ) &&
                                     !compileTimeCompilation.HasImplicitConversion( t, transitiveFabricType ) )
                            .Select( t => t.GetReflectionName().AssertNotNull() )
                            .ToList();

                        var transitiveFabricTypes = compileTimeCompilation.Assembly.GetTypes()
                            .Where( t => compileTimeCompilation.HasImplicitConversion( t, transitiveFabricType ) )
                            .Select( t => t.GetReflectionName().AssertNotNull() )
                            .ToList();

                        var compilerPlugInTypes = compileTimeCompilation.Assembly.GetTypes()
                            .Where( t => t.GetAttributes().Any( a => a is { AttributeClass: { Name: nameof(MetalamaPlugInAttribute) } } ) )
                            .Select( t => t.GetReflectionName().AssertNotNull() )
                            .ToList();

                        var otherTemplateTypes = compileTimeCompilation.Assembly.GetTypes()
                            .Where( t => compileTimeCompilation.HasImplicitConversion( t, templateProviderType ) )
                            .Select( t => t.GetReflectionName().AssertNotNull() )
                            .ToList();

                        var manifest = new CompileTimeProjectManifest(
                            runTimeCompilation.Assembly.Identity.ToString(),
                            compileTimeCompilation.AssemblyName!,
                            runTimeCompilation.GetTargetFramework()?.ToString() ?? "",
                            aspectTypes,
                            compilerPlugInTypes,
                            fabricTypes,
                            transitiveFabricTypes,
                            otherTemplateTypes,
                            referencedProjects.Select( r => r.RunTimeIdentity.GetDisplayName() ).ToList(),
                            sourceHash,
                            textMapDirectory.FilesByTargetPath.Values.Select( f => new CompileTimeFile( f ) ).ToImmutableList() );

                        project = CompileTimeProject.Create(
                            this._serviceProvider,
                            this._domain,
                            runTimeCompilation.Assembly.Identity,
                            compileTimeCompilation.Assembly.Identity,
                            referencedProjects,
                            manifest,
                            outputPaths.Pe,
                            outputPaths.Directory,
                            name => textMapDirectory.GetByName( name ) );

                        this._logger.Trace?.Log( $"Writing manifest to '{outputPaths.Manifest}'." );

                        using ( var manifestStream = File.Create( outputPaths.Manifest ) )
                        {
                            manifest.Serialize( manifestStream );
                        }
                    }
                }

                this._cache.Add( projectHash, project );
            }

            return true;
        }

        private (ulong SourceHash, ulong ProjectHash, OutputPaths OutputPaths) GetPreCacheProjectInfo(
            Compilation runTimeCompilation,
            IReadOnlyList<SyntaxTree> sourceTreesWithCompileTimeCode,
            IReadOnlyList<CompileTimeProject> referencedProjects,
            StringBuilder? log = null )
        {
            var targetFramework = runTimeCompilation.GetTargetFramework();

            var sourceHash = ComputeSourceHash( targetFramework, sourceTreesWithCompileTimeCode, log );
            var projectHash = ComputeProjectHash( referencedProjects, sourceHash, log );

            var outputPaths = this.GetOutputPaths( runTimeCompilation.AssemblyName!, targetFramework, projectHash );

            return (sourceHash, projectHash, outputPaths);
        }

        private record OutputPaths( string Directory, string Pe, string Pdb, string Manifest, string CompileTimeAssemblyName );

        private OutputPaths GetOutputPaths( string runTimeAssemblyName, FrameworkName? targetFramework, ulong projectHash )
        {
            if ( runTimeAssemblyName.StartsWith( _compileTimeAssemblyPrefix, StringComparison.Ordinal ) )
            {
                throw new ArgumentOutOfRangeException( nameof(runTimeAssemblyName) );
            }

            // Note: we must generate file paths that are smaller than 256 characters.

            // Get a shorter name for the target framework.
            string targetFrameworkName;

            if ( targetFramework != null )
            {
                targetFrameworkName = "";

                var splitByComma = targetFramework.FullName.Split( ',' );

                for ( var partIndex = 0; partIndex < splitByComma.Length; partIndex++ )
                {
                    var namePart = splitByComma[partIndex];
                    var splitByEqual = namePart.Split( '=' );

                    string part;

                    if ( splitByEqual.Length == 1 )
                    {
                        part = splitByEqual[0].ToLowerInvariant();
                    }
                    else
                    {
                        part = splitByEqual[1].ToLowerInvariant();
                    }

                    if ( partIndex == 1 )
                    {
                        part = part.TrimStart( 'v' );
                    }

                    if ( partIndex > 1 )
                    {
                        targetFrameworkName += "-";
                    }

                    targetFrameworkName += part;
                }
            }
            else
            {
                targetFrameworkName = "unspecified";
            }

            // Get a shorter assembly name.
            string shortAssemblyName;

            if ( runTimeAssemblyName.Length > 32 )
            {
                // It does not matter if we put several assemblies in the same directory because we include a unique hash of the full name in a subdirectory anyway.
                shortAssemblyName = runTimeAssemblyName.Substring( 0, 32 );
            }
            else
            {
                shortAssemblyName = runTimeAssemblyName;
            }

            // Get the directory name.
            var hash = projectHash.ToString( "x16", CultureInfo.InvariantCulture );

            var directory = Path.Combine(
                this._pathOptions.CompileTimeProjectCacheDirectory,
                shortAssemblyName,
                targetFrameworkName,
                hash );

            // Make sure that the base path is short enough. There should be 16 characters left.
            var remainingPathLength = 256 - directory.Length;

            if ( remainingPathLength < 16 )
            {
                throw new InvalidOperationException( $"The temporary path '{this._pathOptions.CompileTimeProjectCacheDirectory}' is too long." );
            }

            var baseCompileTimeAssemblyName = $"{_compileTimeAssemblyPrefix}{runTimeAssemblyName}";
            var maxLength = remainingPathLength - 8 /* hash */ - 1 /* _ */ - 4 /* .dll */;

            if ( baseCompileTimeAssemblyName.Length > maxLength )
            {
                baseCompileTimeAssemblyName = baseCompileTimeAssemblyName.Substring( 0, maxLength );
            }

            var compileTimeAssemblyName = baseCompileTimeAssemblyName + "_" + hash;

            var pe = Path.Combine( directory, compileTimeAssemblyName + ".dll" );
            var pdb = Path.ChangeExtension( pe, ".pdb" );
            var manifest = Path.Combine( directory, "manifest.json" );

            return new OutputPaths( directory, pe, pdb, manifest, compileTimeAssemblyName );
        }

        /// <summary>
        /// Tries to compile (to a binary image) a project given its manifest and syntax trees. 
        /// </summary>
        public bool TryCompileDeserializedProject(
            string runTimeAssemblyName,
            FrameworkName? targetFramework,
            IReadOnlyList<SyntaxTree> syntaxTrees,
            ulong syntaxTreeHash,
            IReadOnlyList<CompileTimeProject> referencedProjects,
            IDiagnosticAdder diagnosticAdder,
            CancellationToken cancellationToken,
            out string assemblyPath,
            out string? sourceDirectory )
        {
            this._logger.Trace?.Log( $"TryCompileDeserializedProject( '{runTimeAssemblyName}' )" );
            var compileTimeAssemblyName = ComputeProjectHash( referencedProjects, syntaxTreeHash );

            var outputPaths = this.GetOutputPaths( runTimeAssemblyName, targetFramework, compileTimeAssemblyName );

            var compilation = this.CreateEmptyCompileTimeCompilation( outputPaths.CompileTimeAssemblyName, referencedProjects )
                .AddSyntaxTrees( syntaxTrees );

            assemblyPath = outputPaths.Pe;
            sourceDirectory = outputPaths.Directory;

            using ( this.WithLock( outputPaths.CompileTimeAssemblyName ) )
            {
                if ( File.Exists( outputPaths.Pe ) )
                {
                    // If the file already exists, given that it has a strong hash, it means that the assembly has already been 
                    // emitted and it does not need to be done a second time.

                    this._logger.Trace?.Log( $"TryCompileDeserializedProject( '{runTimeAssemblyName}' ): '{outputPaths.Pe}' already exists." );

                    return true;
                }
                else
                {
                    return this.TryEmit( outputPaths, compilation, diagnosticAdder, null, cancellationToken );
                }
            }
        }

        private IDisposable WithLock( string compileTimeAssemblyName ) => MutexHelper.WithGlobalLock( compileTimeAssemblyName, this._logger );
    }
}