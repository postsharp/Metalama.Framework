// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using K4os.Hash.xxHash;
using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Maintenance;
using Metalama.Backstage.Utilities;
using Metalama.Compiler;
using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CompileTime.Manifest;
using Metalama.Framework.Engine.CompileTime.Serialization;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Formatting;
using Metalama.Framework.Engine.Observers;
using Metalama.Framework.Engine.Options;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Templating.Mapping;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Utilities.Diagnostics;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Metalama.Framework.Engine.Utilities.Threading;
using Metalama.Framework.Fabrics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Emit;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using System.Text;
using System.Threading;

namespace Metalama.Framework.Engine.CompileTime;

/// <summary>
/// This class is responsible for building a compile-time <see cref="Compilation"/> based on a run-time one.
/// </summary>
internal sealed partial class CompileTimeCompilationBuilder
{
    public const string CompileTimeAssemblyPrefix = "MetalamaCompileTime_";

    private readonly ProjectServiceProvider _serviceProvider;
    private readonly CompileTimeDomain _domain;
    private readonly Dictionary<ulong, CompileTimeProject> _cache = new();
    private readonly IProjectOptions? _projectOptions;
    private readonly ICompileTimeCompilationBuilderObserver? _observer;
    private readonly ICompileTimeAssemblyBinaryRewriter? _rewriter;
    private readonly ILogger _logger;
    private readonly OutputPathHelper _outputPathHelper;
    private readonly ExecutionScenario _executionScenario;
    private readonly ITaskRunner _taskRunner;

    private static readonly Lazy<ImmutableDictionary<string, string>> _predefinedTypesSyntaxTree = new( GetPredefinedSyntaxTrees );

    private static ImmutableDictionary<string, string> GetPredefinedSyntaxTrees()
    {
        const string prefix = "_Resources_.";

        var assembly = typeof(CompileTimeCompilationBuilder).Assembly;

        // Weirdly enough the assembly prefix of the resource name is not constant; it may or may not include the Roslyn version
        // number.

        var files = assembly.GetManifestResourceNames()
            .Where( n => n.ContainsOrdinal( prefix ) )
            .ToImmutableDictionary(
                name => CompileTimeConstants.GetPrefixedSyntaxTreeName( name.Substring( name.IndexOf( prefix, StringComparison.Ordinal ) + prefix.Length ) )
                        + ".cs",
                name =>
                {
                    using var reader = new StreamReader( assembly.GetManifestResourceStream( name )! );

                    return reader.ReadToEnd();
                } );

        if ( files.IsEmpty )
        {
            throw new AssertionFailedException( "Could not find the predefined syntax trees." );
        }

        return files;
    }

    private static readonly Guid _buildId = AssemblyMetadataReader.GetInstance( typeof(CompileTimeCompilationBuilder).Assembly ).ModuleId;
    private readonly ClassifyingCompilationContextFactory _compilationContextFactory;
    private readonly ITempFileManager _tempFileManager;

    public CompileTimeCompilationBuilder(
        ProjectServiceProvider serviceProvider,
        CompileTimeDomain domain )
    {
        this._serviceProvider = serviceProvider;
        this._domain = domain;
        this._observer = serviceProvider.GetService<ICompileTimeCompilationBuilderObserver>();
        this._rewriter = serviceProvider.Global.GetService<ICompileTimeAssemblyBinaryRewriter>();
        this._projectOptions = serviceProvider.GetService<IProjectOptions>();
        this._compilationContextFactory = serviceProvider.GetRequiredService<ClassifyingCompilationContextFactory>();
        this._logger = serviceProvider.GetLoggerFactory().CompileTime();
        this._tempFileManager = serviceProvider.Underlying.GetRequiredBackstageService<ITempFileManager>();
        this._outputPathHelper = new OutputPathHelper( this._tempFileManager );
        this._executionScenario = serviceProvider.GetService<ExecutionScenario>() ?? ExecutionScenario.CompileTime;
        this._taskRunner = serviceProvider.Global.GetRequiredService<ITaskRunner>();
    }

    private ulong ComputeSourceHash( FrameworkName? targetFramework, IReadOnlyList<SyntaxTree> compileTimeTrees )
    {
        if ( compileTimeTrees.Count == 0 )
        {
            return 0;
        }

        XXH64 h = new();

        // Hash the target framework.
        if ( targetFramework != null )
        {
            this._logger.Trace?.Log( $"SourceHash: TargetFramework='{targetFramework}'" );
            h.Update( targetFramework.FullName );
        }

        // Hash compilation symbols.
        var preprocessorSymbols = compileTimeTrees.SelectAsEnumerable( x => x.Options )
            .SelectMany( x => x.PreprocessorSymbolNames )
            .Distinct()
            .OrderBy( x => x );

        foreach ( var symbol in preprocessorSymbols )
        {
            this._logger.Trace?.Log( $"SourceHash: Symbol='{symbol}'" );
            h.Update( symbol );
        }

        // Hash syntax trees.
        foreach ( var syntaxTree in compileTimeTrees.OrderBy( t => t.FilePath ) )
        {
            // SourceText.Checksum does not seem to return the same thing at compile time than at run time, so we take use our own algorithm.
            var text = syntaxTree.GetText().ToString();
            h.Update( text );

            this._logger.Trace?.Log( $"SourceHash: '{syntaxTree.FilePath}'={string.Join( "", HashUtilities.HashString( text ) )}" );
        }

        var digest = h.Digest();

        return digest;
    }

    private ulong ComputeProjectHash( IEnumerable<CompileTimeProject> referencedProjects, ulong sourceHash, string? redistributionLicenseKey )
    {
        XXH64 h = new();
        h.Update( _buildId );
        this._logger.Trace?.Log( $"ProjectHash: BuildId='{_buildId}'" );

        foreach ( var reference in referencedProjects.OrderBy( r => r.Hash ) )
        {
            h.Update( reference.Hash );
            this._logger.Trace?.Log( $"ProjectHash: '{reference.RunTimeIdentity.Name}'={reference.Hash}" );
        }

        h.Update( sourceHash );
        this._logger.Trace?.Log( $"ProjectHash: Source={sourceHash:x}" );

        h.Update( redistributionLicenseKey );
        this._logger.Trace?.Log( $"RedistributionLicenseKey: {redistributionLicenseKey ?? "null"}" );

        var digest = h.Digest();

        return digest;
    }

    private bool TryCreateCompileTimeCompilation(
        ClassifyingCompilationContext compilationContext,
        IReadOnlyList<SyntaxTree> treesWithCompileTimeCode,
        IReadOnlyCollection<CompileTimeProject> referencedProjects,
        ImmutableArray<UsingDirectiveSyntax> globalUsings,
        OutputPaths outputPaths,
        IDiagnosticAdder diagnosticSink,
        CancellationToken cancellationToken,
        out Compilation? compileTimeCompilation,
        out ILocationAnnotationMap? locationAnnotationMap,
        out TemplateProjectManifest? compilationResultManifest )
    {
        locationAnnotationMap = null;

        // If there is no compile-time tree, there is no need to do anything.
        if ( treesWithCompileTimeCode.Count == 0 )
        {
            compileTimeCompilation = null;
            compilationResultManifest = null;

            return true;
        }

        var runTimeCompilation = compilationContext.SourceCompilation;

        compileTimeCompilation = this.CreateEmptyCompileTimeCompilation( outputPaths.CompileTimeAssemblyName, referencedProjects );
        var serializableTypes = GetSerializableTypes( compilationContext, treesWithCompileTimeCode, cancellationToken );

        var compileTimeCompilationContext = CompilationContextFactory.GetInstance( compileTimeCompilation );

        var templateSymbolManifestBuilder = new TemplateProjectManifestBuilder( compilationContext.SourceCompilation );
        var templateCompiler = new TemplateCompiler( this._serviceProvider, compilationContext, templateSymbolManifestBuilder );

        var produceCompileTimeCodeRewriter = new ProduceCompileTimeCodeRewriter(
            this,
            compilationContext,
            compileTimeCompilationContext,
            serializableTypes,
            globalUsings,
            diagnosticSink,
            templateCompiler,
            referencedProjects,
            templateSymbolManifestBuilder,
            cancellationToken );

        // Creates the new syntax trees. Store them in a dictionary mapping the transformed trees to the source trees.
        var transformedFileGenerator = new TransformedPathGenerator();

        var syntaxTrees = treesWithCompileTimeCode
            .SelectAsList(
                t => (SyntaxTree: t, FileName: Path.GetFileNameWithoutExtension( t.FilePath ),
                      Hash: XXH64.DigestOf( Encoding.UTF8.GetBytes( t.GetText().ToString() ) )) )
            .OrderBy( t => t.FileName )
            .ThenBy( t => t.Hash )
            .Select(
                t =>
                {
                    var compileTimeSyntaxRoot = produceCompileTimeCodeRewriter.Visit( t.SyntaxTree.GetRoot() )
                        .AssertNotNull()
                        .WithAdditionalAnnotations( new SyntaxAnnotation( CompileTimeSyntaxAnnotations.OriginalSyntaxTreePath, t.SyntaxTree.FilePath ) );

                    // Remove all preprocessor trivias.
                    compileTimeSyntaxRoot = new RemovePreprocessorDirectivesRewriter().Visit( compileTimeSyntaxRoot ).AssertNotNull();

                    return CSharpSyntaxTree.Create(
                        (CSharpSyntaxNode) compileTimeSyntaxRoot,
                        SupportedCSharpVersions.DefaultParseOptions,
                        transformedFileGenerator.GetTransformedFilePath( t.FileName, t.Hash ),
                        Encoding.UTF8 );
                } )
            .ToList();

        locationAnnotationMap = templateCompiler.LocationAnnotationMap;
        compilationResultManifest = produceCompileTimeCodeRewriter.GetManifest();

        if ( !produceCompileTimeCodeRewriter.Success )
        {
            this._logger.Warning?.Log( $"TryCreateCompileTimeCompilation( '{runTimeCompilation.AssemblyName}' ): rewriting failed." );

            return false;
        }

        if ( !produceCompileTimeCodeRewriter.FoundCompileTimeCode )
        {
            // This happens if all compile-time code is illegitimate, i.e. was reported as an error and stripped.

            compileTimeCompilation = null;

            return true;
        }

        compileTimeCompilation = compileTimeCompilation.AddSyntaxTrees( syntaxTrees );
        compileTimeCompilation = new RemoveInvalidUsingRewriter( compileTimeCompilation ).VisitTrees( compileTimeCompilation );

        if ( this._projectOptions is { FormatCompileTimeCode: true } )
        {
            var compilation = compileTimeCompilation;
            var formattedCompilation = this._taskRunner.RunSynchronously( () => OutputCodeFormatter.FormatAllAsync( compilation, cancellationToken ) );

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

    public static bool TryParseCompileTimeAssemblyName( string assemblyName, [NotNullWhen( true )] out string? runTimeAssemblyName )
    {
        if ( assemblyName.StartsWith( CompileTimeAssemblyPrefix, StringComparison.OrdinalIgnoreCase ) )
        {
            var parsedAssemblyName = new AssemblyName( assemblyName );
            var shortName = parsedAssemblyName.Name.AssertNotNull();

            runTimeAssemblyName = shortName.Substring(
                CompileTimeAssemblyPrefix.Length,
                shortName.Length - CompileTimeAssemblyPrefix.Length - 17 );

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
        var assemblyLocator = this._serviceProvider.GetReferenceAssemblyLocator();

        var parseOptions = new CSharpParseOptions( preprocessorSymbols: new[] { "NETSTANDARD_2_0" }, languageVersion: SupportedCSharpVersions.Default );

        var standardReferences = assemblyLocator.StandardCompileTimeMetadataReferences;

        var predefinedSyntaxTrees =
            _predefinedTypesSyntaxTree.Value.SelectAsEnumerable( x => CSharpSyntaxTree.ParseText( x.Value, parseOptions, x.Key, Encoding.UTF8 ) );

        return CSharpCompilation.Create(
                assemblyName,
                predefinedSyntaxTrees,
                standardReferences,
                new CSharpCompilationOptions( OutputKind.DynamicallyLinkedLibrary, deterministic: true ) )
            .AddReferences(
                referencedProjects
                    .Where( r => r is { IsEmpty: false, IsFramework: false } )
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

                var text = compileTimeSyntaxTree.GetText().ToString();

                this._logger.Trace?.Log( $"Writing code to '{path}'." );

                // Write the file in a retry loop to handle locks. It seems there are still file lock issues
                // despite the Mutex. 
                RetryHelper.RetryWithLockDetection(
                    path,
                    p => File.WriteAllText( p, text, Encoding.UTF8 ),
                    this._serviceProvider.Underlying,
                    logger: this._logger );

                // Reparse from the text. There is a little performance cost of doing that instead of keeping
                // the parsed syntax tree, however, it has the advantage of detecting syntax errors where we have a valid
                // object tree but an syntax text. These errors are very difficult to diagnose in production situations.
                var newTree = CSharpSyntaxTree.ParseText(
                    text,
                    (CSharpParseOptions?) compileTimeSyntaxTree.Options,
                    path,
                    Encoding.UTF8 );

                // Copy annotations on the root.
                if ( compileTimeSyntaxTree.GetRoot().HasAnnotations( CompileTimeSyntaxAnnotations.OriginalSyntaxTreePath ) )
                {
                    newTree = newTree.WithRootAndOptions( compileTimeSyntaxTree.GetRoot().CopyAnnotationsTo( newTree.GetRoot() ), newTree.Options );
                }

                compileTimeCompilation = compileTimeCompilation.ReplaceSyntaxTree( compileTimeSyntaxTree, newTree );
            }

            this._logger.Trace?.Log( $"Writing binary to '{outputPaths.Pe}'." );

            EmitResult? emitResult = null;

            if ( this._rewriter != null )
            {
                // Metalama.Try defines a binary rewriter to inject Unbreakable.

                MemoryStream memoryStream = new();
                emitResult = compileTimeCompilation.Emit( memoryStream, options: emitOptions, cancellationToken: cancellationToken );

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
                        // We don't write the PE stream directly to the final file because this operation is not atomic.
                        // Instead, we write to a temporary file, and then we move this file to the final destination, because
                        // moving a file is an atomic operation.

                        var tempPeFileName = Path.ChangeExtension( outputPaths.Pe, "tmp" );

                        using ( var peStream = File.Create( tempPeFileName ) )
                        using ( var pdbStream = File.Create( outputPaths.Pdb ) )
                        {
                            emitResult = compileTimeCompilation.Emit( peStream, pdbStream, options: emitOptions, cancellationToken: cancellationToken );
                        }

                        File.Move( tempPeFileName, outputPaths.Pe );
                    },
                    this._serviceProvider.Underlying,
                    logger: this._logger );
            }

            this._observer?.OnCompileTimeCompilationEmit( emitResult!.Diagnostics );

            // Reports a diagnostic in the original syntax tree.
            void ReportDiagnostics( IEnumerable<Diagnostic> diagnostics )
            {
                foreach ( var diagnostic in diagnostics )
                {
                    textMapDirectory ??= TextMapDirectory.Load( outputDirectory );

                    var transformedPath = diagnostic.Location.SourceTree?.FilePath;

                    if ( !string.IsNullOrEmpty( transformedPath ) && textMapDirectory.TryGetMapFile( transformedPath, out var mapFile ) )
                    {
                        var location = mapFile.GetSourceLocation( diagnostic.Location.SourceSpan );

                        var relocatedDiagnostic = Diagnostic.Create(
                            diagnostic.Id,
                            diagnostic.Descriptor.Category,
                            new NonLocalizedString( diagnostic.GetLocalizedMessage() ),
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
                    this._tempFileManager.GetTempDirectory( "CompileTimeTroubleshooting", CleanUpStrategy.Always ),
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
                    var deletedDirectory = Path.Combine( Path.GetDirectoryName( outputDirectory )!, Path.GetFileName( outputDirectory ) + ".del" );

                    RetryHelper.RetryWithLockDetection(
                        files,
                        () =>
                        {
                            if ( Directory.Exists( outputDirectory ) )
                            {
                                // To delete the directory atomically, rename it.
                                Directory.Move( outputDirectory, deletedDirectory );
                                Directory.Delete( deletedDirectory, true );
                            }
                        },
                        this._serviceProvider.Underlying );
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

    private static (IReadOnlyList<SyntaxTree> SyntaxTrees, ImmutableArray<UsingDirectiveSyntax> GlobalUsings) GetCompileTimeArtifacts(
        ClassifyingCompilationContext compilationContext,
        IReadOnlyList<SyntaxTree>? compileTimeTreesHint,
        CancellationToken cancellationToken )
    {
        var runTimeCompilation = compilationContext.SourceCompilation;

        List<SyntaxTree> compileTimeTrees = new();
        var globalUsings = GetUsingsFromOptions( runTimeCompilation );
        var classifier = compilationContext.SymbolClassifier;

        var trees = compileTimeTreesHint ?? runTimeCompilation.SyntaxTrees;

        var semanticModelProvider = runTimeCompilation.GetSemanticModelProvider();

        foreach ( var tree in trees )
        {
            FindCompileTimeCodeVisitor visitor = new( semanticModelProvider.GetSemanticModel( tree, true ), classifier, cancellationToken );
            visitor.Visit( tree.GetRoot() );

            if ( visitor.HasCompileTimeCode )
            {
                compileTimeTrees.Add( tree );
            }

            globalUsings.AddRange( visitor.GlobalUsings );
        }

        return (compileTimeTrees, globalUsings.ToImmutableArray());
    }

    private static IReadOnlyList<SerializableTypeInfo> GetSerializableTypes(
        ClassifyingCompilationContext runTimeCompilationContext,
        IEnumerable<SyntaxTree> compileTimeSyntaxTrees,
        CancellationToken cancellationToken )
    {
        var allSerializableTypes = new Dictionary<ISymbol, SerializableTypeInfo>( runTimeCompilationContext.CompilationContext.SymbolComparer );

        void OnSerializableTypeDiscovered( SerializableTypeInfo type )
        {
            if ( allSerializableTypes.TryGetValue( type.Type, out var existingType ) )
            {
                existingType.SerializedMembers.AddRange( type.SerializedMembers );
            }
            else
            {
                allSerializableTypes[type.Type] = type;
            }
        }

        var semanticModelProvider = runTimeCompilationContext.SemanticModelProvider;

        foreach ( var tree in compileTimeSyntaxTrees )
        {
            var visitor = new CollectSerializableTypesVisitor(
                runTimeCompilationContext,
                semanticModelProvider.GetSemanticModel( tree, true ),
                OnSerializableTypeDiscovered,
                cancellationToken );

            visitor.Visit( tree.GetRoot() );
        }

        return allSerializableTypes.Values.ToList();
    }

    internal bool TryGetCompileTimeProject(
        Compilation compilation,
        ProjectLicenseInfo? projectLicenseInfo,
        IReadOnlyList<SyntaxTree>? compileTimeTreesHint,
        IReadOnlyList<CompileTimeProject> referencedProjects,
        IDiagnosticAdder diagnosticSink,
        bool cacheOnly,
        out CompileTimeProject? project,
        CancellationToken cancellationToken )
        => this.TryGetCompileTimeProject(
            this._compilationContextFactory.GetInstance( compilation ),
            projectLicenseInfo,
            compileTimeTreesHint,
            referencedProjects,
            diagnosticSink,
            cacheOnly,
            out project,
            cancellationToken );

    /// <summary>
    /// Tries to create a compile-time <see cref="Compilation"/> given a run-time <see cref="Compilation"/>.
    /// </summary>
    internal bool TryGetCompileTimeProject(
        ClassifyingCompilationContext compilationContext,
        ProjectLicenseInfo? projectLicenseInfo,
        IReadOnlyList<SyntaxTree>? compileTimeTreesHint,
        IReadOnlyList<CompileTimeProject> referencedProjects,
        IDiagnosticAdder diagnosticSink,
        bool cacheOnly,
        out CompileTimeProject? project,
        CancellationToken cancellationToken )
    {
        var runTimeCompilation = compilationContext.SourceCompilation;

        // If the compilation does not reference Metalama.Framework, do not create a compile-time project.
        if ( !runTimeCompilation.References.OfType<PortableExecutableReference>()
                .Any(
                    p => p.FilePath != null && Path.GetFileNameWithoutExtension( p.FilePath )
                        .Equals( "Metalama.Framework", StringComparison.OrdinalIgnoreCase ) ) )
        {
            this._logger.Trace?.Log( $"TryGetCompileTimeProject( '{runTimeCompilation.AssemblyName}' ) : no reference to Metalama.Framework" );
            project = null;

            return true;
        }

        var compileTimeArtifacts = GetCompileTimeArtifacts( compilationContext, compileTimeTreesHint, cancellationToken );

        return this.TryGetCompileTimeProjectImpl(
            compilationContext,
            projectLicenseInfo,
            compileTimeArtifacts.SyntaxTrees,
            referencedProjects,
            compileTimeArtifacts.GlobalUsings,
            diagnosticSink,
            cacheOnly,
            out project,
            cancellationToken );
    }

    private bool TryGetCompileTimeProjectFromCache(
        Compilation runTimeCompilation,
        IReadOnlyList<CompileTimeProject> referencedProjects,
        OutputPaths outputPaths,
        ulong projectHash,
        [NotNullWhen( true )] out CompileTimeProject? project,
        CacheableTemplateDiscoveryContextProvider? cacheableTemplateDiscoveryContextProvider )
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
            FullPathTextMapFileProvider.Instance,
            cacheableTemplateDiscoveryContextProvider );

        this._cache.Add( projectHash, project );

        return true;
    }

    private bool TryGetCompileTimeProjectImpl(
        ClassifyingCompilationContext compilationContext,
        ProjectLicenseInfo? projectLicenseInfo,
        IReadOnlyList<SyntaxTree> sourceTreesWithCompileTimeCode,
        IReadOnlyList<CompileTimeProject> referencedProjects,
        ImmutableArray<UsingDirectiveSyntax> globalUsings,
        IDiagnosticAdder diagnosticSink,
        bool cacheOnly,
        out CompileTimeProject? project,
        CancellationToken cancellationToken )
    {
        var runTimeCompilation = compilationContext.SourceCompilation;

        // Check the in-process cache.
        var (sourceHash, projectHash, outputPaths) =
            this.GetPreCacheProjectInfo( runTimeCompilation, sourceTreesWithCompileTimeCode, referencedProjects, projectLicenseInfo );

        void ReportCachedDiagnostics( CompileTimeProject project )
        {
            var diagnostics = project.Manifest.AssertNotNull().Diagnostics;

            if ( diagnostics?.Any() == true )
            {
                var sourceTreesForDiagnostics = sourceTreesWithCompileTimeCode.OrderBy( t => t.FilePath ).ToArray();

                diagnosticSink.Report( diagnostics.SelectAsEnumerable( d => d.ToDiagnostic( sourceTreesForDiagnostics ) ) );
            }
        }

        if ( this.TryGetCompileTimeProjectFromCache(
                runTimeCompilation,
                referencedProjects,
                outputPaths,
                projectHash,
                out project,
                null ) )
        {
            ReportCachedDiagnostics( project );

            return true;
        }

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
                    out project,
                    null ) )
            {
                ReportCachedDiagnostics( project );

                // Coverage: ignore (this depends on a multi-threaded condition)
                return true;
            }

            var diagnostics = new List<Diagnostic>();

            // Without this local function, the closure for this method causes a memory leak.
            static DiagnosticAdderAdapter CreateDiagnosticAdder( IDiagnosticAdder diagnosticSink, List<Diagnostic> diagnostics )
                => new(
                    diagnostic =>
                    {
                        // Report diagnostics to the current sink and also store them for the cache.
                        diagnosticSink.Report( diagnostic );
                        diagnostics.Add( diagnostic );
                    } );

            var diagnosticAdder = CreateDiagnosticAdder( diagnosticSink, diagnostics );

            // Generate the C# compilation.
            if ( !this.TryCreateCompileTimeCompilation(
                    compilationContext,
                    sourceTreesWithCompileTimeCode,
                    referencedProjects,
                    globalUsings,
                    outputPaths,
                    diagnosticAdder,
                    cancellationToken,
                    out var compileTimeCompilation,
                    out var locationAnnotationMap,
                    out var compilationResultManifest ) )
            {
                project = null;

                this._logger.Trace?.Log( $"TryCreateCompileTimeCompilation( '{runTimeCompilation.AssemblyName}' ): TryCreateCompileTimeCompilation." );

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

                    this._logger.Trace?.Log( $"TryGetCompileTimeProjectImpl( '{runTimeCompilation.AssemblyName}' ): emit failed." );

                    return false;
                }

                textMapDirectory.Write( outputPaths.Directory );

                var aspectType = compileTimeCompilation.GetTypeByMetadataName( typeof(IAspect).FullName.AssertNotNull() );
                var fabricType = compileTimeCompilation.GetTypeByMetadataName( typeof(Fabric).FullName.AssertNotNull() );
                var transitiveFabricType = compileTimeCompilation.GetTypeByMetadataName( typeof(TransitiveProjectFabric).FullName.AssertNotNull() );
                var templateProviderType = compileTimeCompilation.GetTypeByMetadataName( typeof(ITemplateProvider).FullName.AssertNotNull() );

                var aspectTypes = compileTimeCompilation.Assembly.GetAllTypes()
                    .Where( t => compileTimeCompilation.HasImplicitConversion( t, aspectType ) )
                    .Select( t => t.GetReflectionFullName().AssertNotNull() )
                    .ToList();

                var fabricTypes = compileTimeCompilation.Assembly.GetTypes()
                    .Where(
                        t => compileTimeCompilation.HasImplicitConversion( t, fabricType ) &&
                             !compileTimeCompilation.HasImplicitConversion( t, transitiveFabricType ) )
                    .Select( t => t.GetReflectionFullName().AssertNotNull() )
                    .ToList();

                var transitiveFabricTypes = compileTimeCompilation.Assembly.GetTypes()
                    .Where( t => compileTimeCompilation.HasImplicitConversion( t, transitiveFabricType ) )
                    .Select( t => t.GetReflectionFullName().AssertNotNull() )
                    .ToList();

                var compilerPlugInTypes = compileTimeCompilation.Assembly.GetAllTypes()
                    .Where( t => t.GetAttributes().Any( a => a is { AttributeClass.Name: nameof(MetalamaPlugInAttribute) } ) )
                    .Select( t => t.GetReflectionFullName().AssertNotNull() )
                    .ToList();

                var otherTemplateTypes = compileTimeCompilation.Assembly.GetAllTypes()
                    .Where( t => compileTimeCompilation.HasImplicitConversion( t, templateProviderType ) )
                    .Select( t => t.GetReflectionFullName().AssertNotNull() )
                    .ToList();

                Dictionary<string, int>? sourceFilePathIndexes = null;

                if ( diagnostics.Any() )
                {
                    sourceFilePathIndexes = sourceTreesWithCompileTimeCode
                        .Select( ( tree, index ) => (tree.FilePath, index) )
                        .OrderBy( x => x.FilePath )
                        .ToDictionary( x => x.FilePath, x => x.index );
                }

                var manifest = new CompileTimeProjectManifest(
                    runTimeCompilation.Assembly.Identity.ToString(),
                    compileTimeCompilation.AssemblyName!,
                    runTimeCompilation.GetTargetFramework()?.ToString() ?? "",
                    aspectTypes,
                    compilerPlugInTypes,
                    fabricTypes,
                    transitiveFabricTypes,
                    otherTemplateTypes,
                    referencedProjects.SelectAsImmutableArray( r => r.RunTimeIdentity.GetDisplayName() ),
                    compilationResultManifest,
                    projectLicenseInfo?.RedistributionLicenseKey,
                    sourceHash,
                    textMapDirectory.FilesByTargetPath.Values.Select( f => new CompileTimeFileManifest( f ) ).ToArray(),
                    diagnostics.SelectAsArray( d => new CompileTimeDiagnosticManifest( d, sourceFilePathIndexes! ) ) );

                project = CompileTimeProject.Create(
                    this._serviceProvider,
                    this._domain,
                    runTimeCompilation.Assembly.Identity,
                    compileTimeCompilation.Assembly.Identity,
                    referencedProjects,
                    manifest,
                    outputPaths.Pe,
                    outputPaths.Directory,
                    textMapDirectory,
                    null );

                this._logger.Trace?.Log( $"Writing manifest to '{outputPaths.Manifest}'." );

                using ( var manifestStream = File.Create( outputPaths.Manifest ) )
                {
                    manifest.Serialize( manifestStream );
                }
            }
        }

        this._cache.Add( projectHash, project );

        return true;
    }

    private (ulong SourceHash, ulong ProjectHash, OutputPaths OutputPaths) GetPreCacheProjectInfo(
        Compilation runTimeCompilation,
        IReadOnlyList<SyntaxTree> sourceTreesWithCompileTimeCode,
        IEnumerable<CompileTimeProject> referencedProjects,
        ProjectLicenseInfo? projectLicenseInfo )
    {
        var targetFramework = runTimeCompilation.GetTargetFramework();

        var sourceHash = this.ComputeSourceHash( targetFramework, sourceTreesWithCompileTimeCode );
        var projectHash = this.ComputeProjectHash( referencedProjects, sourceHash, projectLicenseInfo?.RedistributionLicenseKey );

        var outputPaths = this._outputPathHelper.GetOutputPaths( runTimeCompilation.AssemblyName!, targetFramework, projectHash );

        return (sourceHash, projectHash, outputPaths);
    }

    /// <summary>
    /// Tries to compile (to a binary image) a project given its manifest and syntax trees. 
    /// </summary>
    public bool TryCompileDeserializedProject(
        string runTimeAssemblyName,
        FrameworkName? targetFramework,
        IReadOnlyList<SyntaxTree> syntaxTrees,
        ulong syntaxTreeHash,
        string? redistributionLicenseKey,
        IReadOnlyList<CompileTimeProject> referencedProjects,
        IDiagnosticAdder diagnosticAdder,
        CancellationToken cancellationToken,
        out string assemblyPath,
        out string? sourceDirectory )
    {
        this._logger.Trace?.Log( $"TryCompileDeserializedProject( '{runTimeAssemblyName}' )" );
        var projectHash = this.ComputeProjectHash( referencedProjects, syntaxTreeHash, redistributionLicenseKey );

        var outputPaths = this._outputPathHelper.GetOutputPaths( runTimeAssemblyName, targetFramework, projectHash );

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