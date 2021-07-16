// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Formatting;
using Caravela.Framework.Impl.Options;
using Caravela.Framework.Impl.Templating;
using Caravela.Framework.Impl.Templating.Mapping;
using Caravela.Framework.Impl.Utilities;
using Caravela.Framework.Sdk;
using K4os.Hash.xxHash;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Caravela.Framework.Impl.CompileTime
{
    /// <summary>
    /// This class is responsible for building a compile-time <see cref="Compilation"/> based on a run-time one.
    /// </summary>
    internal partial class CompileTimeCompilationBuilder
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly CompileTimeDomain _domain;
        private readonly Dictionary<ulong, CompileTimeProject> _cache = new();
        private readonly IDirectoryOptions _directoryOptions;
        private readonly IProjectOptions? _projectOptions;
        private readonly ICompileTimeCompilationBuilderSpy? _spy;
        private readonly ICompileTimeAssemblyBinaryRewriter? _rewriter;

        private static readonly Guid _buildId = AssemblyMetadataReader.GetInstance( typeof(CompileTimeCompilationBuilder).Assembly ).ModuleId;

        public const string ResourceName = "Caravela.CompileTimeAssembly";

        public CompileTimeCompilationBuilder( IServiceProvider serviceProvider, CompileTimeDomain domain )
        {
            this._directoryOptions = serviceProvider.GetService<IDirectoryOptions>();
            this._serviceProvider = serviceProvider;
            this._domain = domain;
            this._spy = serviceProvider.GetOptionalService<ICompileTimeCompilationBuilderSpy>();
            this._rewriter = serviceProvider.GetOptionalService<ICompileTimeAssemblyBinaryRewriter>();
            this._projectOptions = serviceProvider.GetOptionalService<IProjectOptions>();
        }

        private static ulong ComputeSourceHash( IReadOnlyList<SyntaxTree> compileTimeTrees, StringBuilder? log = null )
        {
            log?.AppendLine( nameof(ComputeSourceHash) );
            XXH64 h = new();

            foreach ( var syntaxTree in compileTimeTrees.OrderBy( t => t.FilePath ) )
            {
                // SourceText.Checksum does not seem to return the same thing at compile time than at run time, so we take use our own algorithm.
                var text = syntaxTree.GetText().ToString();
                h.Update( text );

                log?.AppendLine( $"Source:{syntaxTree.FilePath}={string.Join( "", HashUtilities.HashString( text ) )}" );
            }

            var digest = h.Digest();
            log?.AppendLine( $"Digest:{digest:x}" );

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
            log?.AppendLine( $"BuildId={_buildId}" );

            foreach ( var reference in referencedProjects.OrderBy( r => r.Hash ) )
            {
                h.Update( reference.Hash );
                log?.AppendLine( $"Reference:={reference.RunTimeIdentity.Name}={reference.Hash}" );
            }

            h.Update( sourceHash );
            log?.AppendLine( $"Source:={sourceHash:x}" );

            var digest = h.Digest();
            log?.AppendLine( $"Digest:{digest:x}" );

            return digest;
        }

        private bool TryCreateCompileTimeCompilation(
            Compilation runTimeCompilation,
            IReadOnlyList<SyntaxTree> treesWithCompileTimeCode,
            IEnumerable<CompileTimeProject> referencedProjects,
            ulong hash,
            IDiagnosticAdder diagnosticSink,
            CancellationToken cancellationToken,
            out Compilation? compileTimeCompilation,
            out ILocationAnnotationMap? locationAnnotationMap,
            out Dictionary<string, SyntaxTree>? syntaxTreeMap )
        {
            locationAnnotationMap = null;

            // If there is no compile-time tree, there is no need to do anything.
            if ( treesWithCompileTimeCode.Count == 0 )
            {
                compileTimeCompilation = null;
                syntaxTreeMap = null;

                return true;
            }

            // Validate the code (some validations are not done by the template compiler).
            foreach ( var syntaxTree in treesWithCompileTimeCode )
            {
                var semanticModel = runTimeCompilation.GetSemanticModel( syntaxTree );
                TemplatingCodeValidator.Validate( semanticModel, diagnosticSink.Report, this._serviceProvider, false, false, cancellationToken );
            }

            var assemblyName = GetCompileTimeAssemblyName( runTimeCompilation.AssemblyName!, hash );
            compileTimeCompilation = this.CreateEmptyCompileTimeCompilation( assemblyName, referencedProjects );

            var templateCompiler = new TemplateCompiler( this._serviceProvider, runTimeCompilation );

            var produceCompileTimeCodeRewriter = new ProduceCompileTimeCodeRewriter(
                runTimeCompilation,
                compileTimeCompilation,
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
                              .WithFilePath( GetTransformedFilePath( t.FilePath ) ),
                          SourceTree: t) )
                .ToList();

            syntaxTreeMap = syntaxTrees.ToDictionary( t => t.TransformedTree.FilePath, t => t.SourceTree );

            locationAnnotationMap = templateCompiler.LocationAnnotationMap;

            if ( !produceCompileTimeCodeRewriter.Success )
            {
                return false;
            }

            if ( !produceCompileTimeCodeRewriter.FoundCompileTimeCode )
            {
                compileTimeCompilation = null;

                return true;
            }

            compileTimeCompilation = compileTimeCompilation.AddSyntaxTrees( syntaxTrees.Select( t => t.TransformedTree ) );

            this._spy?.ReportCompileTimeCompilation( compileTimeCompilation );

            compileTimeCompilation = new RemoveInvalidUsingRewriter( compileTimeCompilation ).VisitTrees( compileTimeCompilation );

            if ( this._projectOptions is { FormatCompileTimeCode: true } && OutputCodeFormatter.CanFormat )
            {
                compileTimeCompilation = OutputCodeFormatter.FormatAll( compileTimeCompilation );
            }

            return true;
        }

        private static string GetTransformedFilePath( string originalFilePath )
        {
            // Find a decent and unique name.
            var transformedFileName = !string.IsNullOrWhiteSpace( originalFilePath )
                ? Path.GetFileNameWithoutExtension( originalFilePath )
                : "Anonymous";

            transformedFileName += "_" + HashUtilities.HashString( originalFilePath );
            transformedFileName += Path.GetExtension( originalFilePath );

            return transformedFileName;
        }

        internal static string GetCompileTimeAssemblyName(
            string runTimeAssemblyName,
            IEnumerable<CompileTimeProject> referencedProjects,
            ulong sourceHash,
            StringBuilder? log = null )
        {
            var projectHash = ComputeProjectHash( referencedProjects, sourceHash, log );

            return GetCompileTimeAssemblyName( runTimeAssemblyName, projectHash );
        }

        private static string GetCompileTimeAssemblyName( string runTimeAssemblyName, ulong projectHash )
            => $"Caravela_{runTimeAssemblyName}_{projectHash:x16}";

        private CSharpCompilation CreateEmptyCompileTimeCompilation(
            string assemblyName,
            IEnumerable<CompileTimeProject> referencedProjects )
        {
            var assemblyLocator = this._serviceProvider.GetService<ReferenceAssemblyLocator>();

            var standardReferences = assemblyLocator.StandardCompileTimeMetadataReferences;

            return CSharpCompilation.Create(
                    assemblyName,
                    Array.Empty<SyntaxTree>(),
                    standardReferences,
                    new CSharpCompilationOptions( OutputKind.DynamicallyLinkedLibrary, deterministic: true ) )
                .AddReferences(
                    referencedProjects
                        .Where( r => !r.IsEmpty )
                        .Select( r => r.ToMetadataReference() ) );
        }

        private bool TryEmit(
            Compilation compileTimeCompilation,
            IDiagnosticAdder diagnosticSink,
            TextMapDirectory? textMapDirectory,
            CancellationToken cancellationToken )
        {
            var outputInfo = this.GetOutputPaths( compileTimeCompilation.AssemblyName! );

            Logger.Instance?.Write( $"TryEmit( '{compileTimeCompilation.AssemblyName}' )" );

            try
            {
                var emitOptions = new EmitOptions( debugInformationFormat: DebugInformationFormat.PortablePdb );

                // Write the generated files to disk if we should.
                if ( !Directory.Exists( outputInfo.Directory ) )
                {
                    Logger.Instance?.Write( $"Creating directory ( '{outputInfo.Directory}'." );
                    Directory.CreateDirectory( outputInfo.Directory );
                }

                compileTimeCompilation =
                    compileTimeCompilation.WithOptions( compileTimeCompilation.Options.WithOptimizationLevel( OptimizationLevel.Debug ) );

                foreach ( var compileTimeSyntaxTree in compileTimeCompilation.SyntaxTrees )
                {
                    var transformedFileName = Path.Combine( outputInfo.Directory, compileTimeSyntaxTree.FilePath );

                    var path = Path.Combine( outputInfo.Directory, transformedFileName );
                    var text = compileTimeSyntaxTree.GetText();

                    // Write the file in a retry loop to handle locks. It seems there are still file lock issues
                    // despite the Mutex. 
                    RetryHelper.Retry(
                        () =>
                        {
                            using ( var textWriter = new StreamWriter( path, false, Encoding.UTF8 ) )
                            {
                                text.Write( textWriter, cancellationToken );
                            }
                        } );

                    // Update the link to the file path.
                    var newTree = CSharpSyntaxTree.Create(
                        (CSharpSyntaxNode) compileTimeSyntaxTree.GetRoot(),
                        (CSharpParseOptions?) compileTimeSyntaxTree.Options,
                        path,
                        Encoding.UTF8 );

                    compileTimeCompilation = compileTimeCompilation.ReplaceSyntaxTree( compileTimeSyntaxTree, newTree );
                }

                EmitResult emitResult;

                if ( this._rewriter != null )
                {
                    // TryCaravela defines a binary rewriter to inject Unbreakable.

                    MemoryStream memoryStream = new();
                    emitResult = compileTimeCompilation.Emit( memoryStream, null, options: emitOptions, cancellationToken: cancellationToken );

                    if (emitResult.Success)
                    {
                        memoryStream.Seek(0, SeekOrigin.Begin);

                        using (var peStream = File.Create(outputInfo.Pe))
                        {
                            this._rewriter.Rewrite(memoryStream, peStream, outputInfo.Pe );
                        }
                    }

                }
                else
                {
                    using ( var peStream = File.Create( outputInfo.Pe ) )
                    using ( var pdbStream = File.Create( outputInfo.Pdb ) )
                    {
                        emitResult = compileTimeCompilation.Emit( peStream, pdbStream, options: emitOptions, cancellationToken: cancellationToken );
                    }
                }

                // Reports a diagnostic in the original syntax tree.
                void ReportDiagnostics( IEnumerable<Diagnostic> diagnostics )
                {
                    foreach ( var diagnostic in diagnostics )
                    {
                        if ( textMapDirectory == null )
                        {
                            textMapDirectory = TextMapDirectory.Load( outputInfo.Directory );
                        }

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
                            diagnosticSink.Report( diagnostic );
                        }
                    }
                }

                if ( !emitResult.Success )
                {
                    Logger.Instance?.Write(
                        $"TryEmit( '{compileTimeCompilation.AssemblyName}' ): failure: " +
                        string.Join( Environment.NewLine, emitResult.Diagnostics ) );

                    ReportDiagnostics( emitResult.Diagnostics.Where( d => d.Severity >= DiagnosticSeverity.Error ) );

                    DeleteOutputFiles();

                    return false;
                }

                if ( textMapDirectory == null )
                {
                    diagnosticSink.Report( emitResult.Diagnostics );
                }

                Logger.Instance?.Write( $"TryEmit( '{compileTimeCompilation.AssemblyName}' ): success." );

                return emitResult.Success;
            }
            catch ( Exception e )
            {
                Logger.Instance?.Write( e.ToString() );

                DeleteOutputFiles();

                throw;
            }

            void DeleteOutputFiles()
            {
                RetryHelper.Retry(
                    () =>
                    {
                        if ( File.Exists( outputInfo.Pe ) )
                        {
                            File.Delete( outputInfo.Pe );
                        }
                    } );

                RetryHelper.Retry(
                    () =>
                    {
                        if ( File.Exists( outputInfo.Pdb ) )
                        {
                            File.Delete( outputInfo.Pdb );
                        }
                    } );
            }
        }

        private IReadOnlyList<SyntaxTree> GetCompileTimeSyntaxTrees(
            Compilation runTimeCompilation,
            IReadOnlyList<SyntaxTree>? compileTimeTreesHint,
            CancellationToken cancellationToken )
        {
            List<SyntaxTree> compileTimeTrees = new();
            var classifier = this._serviceProvider.GetService<SymbolClassificationService>().GetClassifier( runTimeCompilation );

            var trees = compileTimeTreesHint ?? runTimeCompilation.SyntaxTrees;

            foreach ( var tree in trees )
            {
                FindCompileTimeCodeVisitor visitor = new( runTimeCompilation.GetSemanticModel( tree, true ), classifier, cancellationToken );
                visitor.Visit( tree.GetRoot() );

                if ( visitor.HasCompileTimeCode )
                {
                    compileTimeTrees.Add( tree );
                }
            }

            return compileTimeTrees;
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
            => this.TryGetCompileTimeProjectImpl(
                runTimeCompilation,
                this.GetCompileTimeSyntaxTrees( runTimeCompilation, compileTimeTreesHint, cancellationToken ),
                referencedProjects,
                diagnosticSink,
                cacheOnly,
                cancellationToken,
                out project );

        private bool TryGetCompileTimeProjectFromCache(
            Compilation runTimeCompilation,
            IReadOnlyList<CompileTimeProject> referencedProjects,
            OutputPaths outputPaths,
            string compileTimeAssemblyName,
            ulong projectHash,
            out CompileTimeProject? project )
        {
            Logger.Instance?.Write( $"TryGetCompileTimeProjectFromCache( '{runTimeCompilation}' )" );

            // Look in in-memory cache.
            if ( this._cache.TryGetValue( projectHash, out project ) )
            {
                Logger.Instance?.Write( $"TryGetCompileTimeProjectFromCache( '{runTimeCompilation}' ): found in memory cache." );

                return true;
            }

            // Look on disk.
            if ( !File.Exists( outputPaths.Pe ) || !File.Exists( outputPaths.Manifest ) )
            {
                Logger.Instance?.Write( $"TryGetCompileTimeProjectFromCache( '{runTimeCompilation}' ): not found." );

                project = null;

                return false;
            }

            Logger.Instance?.Write( $"TryGetCompileTimeProjectFromCache( '{runTimeCompilation}' ): found on disk. Deserializing." );

            // Deserialize the manifest.
            if ( CompileTimeProjectManifest.TryDeserialize( File.OpenRead( outputPaths.Manifest ), out var manifest ) )
            {
                project = CompileTimeProject.Create(
                    this._domain,
                    runTimeCompilation.Assembly.Identity,
                    new AssemblyIdentity( compileTimeAssemblyName ),
                    referencedProjects,
                    manifest,
                    outputPaths.Pe,
                    outputPaths.Directory,
                    TextMapFile.ReadForSource );

                this._cache.Add( projectHash, project );

                return true;
            }
            else
            {
                Logger.Instance?.Write( $"Cannot deserialize '{outputPaths.Manifest}'." );

                try
                {
                    File.Delete( outputPaths.Manifest );
                }
                catch ( IOException e )
                {
                    Logger.Instance?.Write( $"Cannot delete '{outputPaths.Manifest}': {e.Message}" );
                }

                project = null;

                return false;
            }
        }

        private bool TryGetCompileTimeProjectImpl(
            Compilation runTimeCompilation,
            IReadOnlyList<SyntaxTree> sourceTreesWithCompileTimeCode,
            IReadOnlyList<CompileTimeProject> referencedProjects,
            IDiagnosticAdder diagnosticSink,
            bool cacheOnly,
            CancellationToken cancellationToken,
            out CompileTimeProject? project )
        {
            // Check the in-process cache.
            var (sourceHash, projectHash, compileTimeAssemblyName, outputPaths) =
                this.GetPreCacheProjectInfo( runTimeCompilation, sourceTreesWithCompileTimeCode, referencedProjects );

            if ( !this.TryGetCompileTimeProjectFromCache(
                runTimeCompilation,
                referencedProjects,
                outputPaths,
                compileTimeAssemblyName,
                projectHash,
                out project ) )
            {
                if ( cacheOnly )
                {
                    // We were asked to get cache projects only. Don't create it.
                    project = null;

                    return false;
                }

                using ( WithLock( compileTimeAssemblyName ) )
                {
                    // Do a second cache lookup within the lock.
                    if ( this.TryGetCompileTimeProjectFromCache(
                        runTimeCompilation,
                        referencedProjects,
                        outputPaths,
                        compileTimeAssemblyName,
                        projectHash,
                        out project ) )
                    {
                        return true;
                    }

                    // Generate the C# compilation.
                    if ( !this.TryCreateCompileTimeCompilation(
                        runTimeCompilation,
                        sourceTreesWithCompileTimeCode,
                        referencedProjects,
                        projectHash,
                        diagnosticSink,
                        cancellationToken,
                        out var compileTimeCompilation,
                        out var locationAnnotationMap,
                        out _ ) )
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
                                this._domain,
                                runTimeCompilation.Assembly.Identity,
                                new AssemblyIdentity( compileTimeAssemblyName ),
                                referencedProjects );
                        }

                        return true;
                    }
                    else
                    {
                        var textMapDirectory = TextMapDirectory.Create( compileTimeCompilation, locationAnnotationMap! );

                        if ( !this.TryEmit( compileTimeCompilation, diagnosticSink, textMapDirectory, cancellationToken ) )
                        {
                            project = null;

                            return false;
                        }

                        textMapDirectory.Write( outputPaths.Directory );

                        var aspectType = compileTimeCompilation.GetTypeByMetadataName( typeof(IAspect).FullName );

                        var manifest = new CompileTimeProjectManifest(
                            compileTimeCompilation.AssemblyName!,
                            compileTimeCompilation.Assembly
                                .GetTypes()
                                .Where( t => compileTimeCompilation.HasImplicitConversion( t, aspectType ) )
                                .Select( t => t.GetReflectionNameSafe() )
                                .ToList(),
                            referencedProjects.Select( r => r.RunTimeIdentity.GetDisplayName() ).ToList(),
                            sourceHash,
                            textMapDirectory.FilesByTargetPath.Values.Select( f => new CompileTimeFile( f ) ).ToImmutableList() );

                        project = CompileTimeProject.Create(
                            this._domain,
                            runTimeCompilation.Assembly.Identity,
                            compileTimeCompilation.Assembly.Identity,
                            referencedProjects,
                            manifest,
                            outputPaths.Pe,
                            outputPaths.Directory,
                            name => textMapDirectory.GetByName( name ) );

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

        private (ulong SourceHash, ulong ProjectHash, string CompileTimeAssemblyName, OutputPaths OutputPaths) GetPreCacheProjectInfo(
            Compilation runTimeCompilation,
            IReadOnlyList<SyntaxTree> sourceTreesWithCompileTimeCode,
            IReadOnlyList<CompileTimeProject> referencedProjects,
            StringBuilder? log = null )
        {
            var sourceHash = ComputeSourceHash( sourceTreesWithCompileTimeCode, log );
            var projectHash = ComputeProjectHash( referencedProjects, sourceHash, log );

            var compileTimeAssemblyName = GetCompileTimeAssemblyName( runTimeCompilation.AssemblyName!, projectHash );
            var outputPaths = this.GetOutputPaths( compileTimeAssemblyName );

            return (sourceHash, projectHash, compileTimeAssemblyName, outputPaths);
        }

        private record OutputPaths( string Directory, string Pe, string Pdb, string Manifest );

        private OutputPaths GetOutputPaths( string compileTimeAssemblyName )
        {
            // We cannot include the full assembly name in the path because we're hitting the max path length.
            var directory = Path.Combine( this._directoryOptions.CompileTimeProjectCacheDirectory, HashUtilities.HashString( compileTimeAssemblyName ) );
            var pe = Path.Combine( directory, compileTimeAssemblyName + ".dll" );
            var pdb = Path.ChangeExtension( pe, ".pdb" );
            var manifest = Path.ChangeExtension( pe, ".manifest" );

            return new OutputPaths( directory, pe, pdb, manifest );
        }

        /// <summary>
        /// Tries to compile (to a binary image) a project given its manifest and syntax trees. 
        /// </summary>
        public bool TryCompileDeserializedProject(
            string runTimeAssemblyName,
            IReadOnlyList<SyntaxTree> syntaxTrees,
            ulong syntaxTreeHash,
            IReadOnlyList<CompileTimeProject> referencedProjects,
            IDiagnosticAdder diagnosticAdder,
            CancellationToken cancellationToken,
            out string assemblyPath,
            out string sourceDirectory )
        {
            Logger.Instance?.Write( $"TryCompileDeserializedProject( '{runTimeAssemblyName}' )" );
            var compileTimeAssemblyName = GetCompileTimeAssemblyName( runTimeAssemblyName, referencedProjects, syntaxTreeHash );

            var outputInfo = this.GetOutputPaths( compileTimeAssemblyName );

            var compilation = this.CreateEmptyCompileTimeCompilation( compileTimeAssemblyName, referencedProjects )
                .AddSyntaxTrees( syntaxTrees );

            assemblyPath = outputInfo.Pe;
            sourceDirectory = outputInfo.Directory;

            using ( WithLock( compileTimeAssemblyName ) )
            {
                if ( File.Exists( outputInfo.Pe ) )
                {
                    // If the file already exists, given that it has a strong hash, it means that the assembly has already been 
                    // emitted and it does not need to be done a second time.

                    Logger.Instance?.Write( $"TryCompileDeserializedProject( '{runTimeAssemblyName}' ): '{outputInfo.Pe}' already exists." );

                    return true;
                }
                else
                {
                    return this.TryEmit( compilation, diagnosticAdder, null, cancellationToken );
                }
            }
        }

        private static IDisposable WithLock( string compileTimeAssemblyName ) => MutexHelper.WithGlobalLock( compileTimeAssemblyName );
    }
}