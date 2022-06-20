// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Utilities;
using Metalama.Framework.Code;
using Metalama.Framework.DesignTime.Pipeline.Diff;
using Metalama.Framework.DesignTime.Utilities;
using Metalama.Framework.Eligibility;
using Metalama.Framework.Engine;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Engine.Pipeline.DesignTime;
using Metalama.Framework.Engine.Pipeline.LiveTemplates;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Metalama.Framework.DesignTime.Pipeline
{
    /// <summary>
    /// The design-time implementation of <see cref="AspectPipeline"/>.
    /// </summary>
    /// Must be public because of testing.
    internal partial class DesignTimeAspectPipeline : BaseDesignTimeAspectPipeline
    {
        private readonly ConditionalWeakTable<Compilation, CompilationResult> _compilationResultCache = new();
        private static readonly string _sourceGeneratorAssemblyName = typeof(DesignTimeAspectPipelineFactory).Assembly.GetName().Name;

        private readonly IFileSystemWatcher? _fileSystemWatcher;

        // This field should not be changed directly, but only through the SetState method.
        private PipelineState _currentState;

        private int _pipelineExecutionCount;

        /// <summary>
        /// Gets the number of times the pipeline has been executed. Useful for testing purposes.
        /// </summary>
        public int PipelineExecutionCount => this._pipelineExecutionCount;

        /// <summary>
        /// Gets an object that can be locked to get exclusive access to
        /// the current instance.
        /// </summary>
        private readonly object _sync = new();

        // ReSharper disable once InconsistentlySynchronizedField
        internal DesignTimeAspectPipelineStatus Status => this._currentState.Status;

        internal event Action<DesignTimePipelineStatusChangedEventArgs>? StatusChanged;

        private void SetState( in PipelineState state )
        {
            var oldStatus = this._currentState.Status;
            this._currentState = state;

            if ( oldStatus != state.Status )
            {
                this.StatusChanged?.Invoke( new DesignTimePipelineStatusChangedEventArgs( this, oldStatus, state.Status ) );
            }
        }

        // It's ok if we return an obsolete project in the use cases of this property.
        // ReSharper disable once InconsistentlySynchronizedField
        private IEnumerable<AspectClass>? AspectClasses => this._currentState.Configuration?.AspectClasses.OfType<AspectClass>();

        public DesignTimeAspectPipeline(
            ServiceProvider serviceProvider,
            CompileTimeDomain domain,
            IEnumerable<MetadataReference> metadataReferences,
            bool isTest )
            : base( serviceProvider.WithProjectScopedServices( metadataReferences ), isTest, domain )
        {
            this.Factory = this.ServiceProvider.GetService<DesignTimeAspectPipelineFactory>();

            this._currentState = new PipelineState( this );

            // The design-time pipeline contains project-scoped services for performance reasons: the pipeline may be called several
            // times with the same compilation.

            if ( string.IsNullOrEmpty( this.ProjectOptions.BuildTouchFile ) )
            {
                return;
            }

            this.Logger.Trace?.Log( $"BuildTouchFile={this.ProjectOptions.BuildTouchFile}" );

            var watchedFilter = "*" + Path.GetExtension( this.ProjectOptions.BuildTouchFile );
            var watchedDirectory = Path.GetDirectoryName( this.ProjectOptions.BuildTouchFile );

            if ( watchedDirectory == null )
            {
                return;
            }

            var fileSystemWatcherFactory = this.ServiceProvider.GetService<IFileSystemWatcherFactory>() ?? new FileSystemWatcherFactory();
            this._fileSystemWatcher = fileSystemWatcherFactory.Create( watchedDirectory, watchedFilter );
            this._fileSystemWatcher.IncludeSubdirectories = false;

            this._fileSystemWatcher.Changed += this.OnOutputDirectoryChanged;
            this._fileSystemWatcher.EnableRaisingEvents = true;
        }

        public event EventHandler? PipelineResumed;

        private void OnOutputDirectoryChanged( object sender, FileSystemEventArgs e )
        {
            if ( e.FullPath != this.ProjectOptions.BuildTouchFile || this.Status != DesignTimeAspectPipelineStatus.Paused )
            {
                return;
            }

            // There was an external build. Touch the files to re-run the analyzer.
            this.Logger.Trace?.Log( $"Detected an external build for project '{this.ProjectOptions.ProjectId}'." );

            this.Resume( true );
        }

        public void Resume( bool touchFiles )
        {
            using ( this.WithLock() )
            {
                if ( this.Status != DesignTimeAspectPipelineStatus.Paused )
                {
                    this.Logger.Trace?.Log( $"A Resume request was requested for project '{this.ProjectOptions.ProjectId}', but the pipeline was not paused." );

                    return;
                }

                var hasRelevantChange = false;
                var filesToTouch = new List<string>();

                foreach ( var file in this._currentState.CompileTimeSyntaxTrees.AssertNotNull() )
                {
                    if ( file.Value == null )
                    {
                        hasRelevantChange = true;

                        if ( touchFiles )
                        {
                            filesToTouch.Add( file.Key );
                        }
                    }
                }

                if ( hasRelevantChange )
                {
                    this.Logger.Trace?.Log(
                        $"Resuming the pipeline for project '{this.ProjectOptions.ProjectId}'. The following files had compile-time changes: {string.Join( ", ", filesToTouch.Select( x => $"'{x}'" ) )} " );

                    this.SetState( new PipelineState( this ) );
                    this.PipelineResumed?.Invoke( this, EventArgs.Empty );

                    if ( this.MustReportPausedPipelineAsErrors )
                    {
                        // Touching the files after having reset the pipeline.
                        foreach ( var file in filesToTouch )
                        {
                            this.Logger.Trace?.Log( $"Touching file '{file}'." );
                            RetryHelper.Retry( () => File.SetLastWriteTimeUtc( file, DateTime.UtcNow ), logger: this.Logger );
                        }
                    }
                }
                else
                {
                    this.Logger.Trace?.Log(
                        $"A Resume request was requested for project '{this.ProjectOptions.ProjectId}', but there was no relevant change." );
                }
            }
        }

        internal bool TryGetConfiguration(
            PartialCompilation compilation,
            IDiagnosticAdder diagnosticAdder,
            bool ignoreStatus,
            CancellationToken cancellationToken,
            [NotNullWhen( true )] out AspectPipelineConfiguration? configuration )
        {
            using ( this.WithLock() )
            {
                var state = this._currentState;
                var success = PipelineState.TryGetConfiguration( ref state, compilation, diagnosticAdder, ignoreStatus, cancellationToken, out configuration );

                this.SetState( state );

                return success;
            }
        }

        public Compilation? LastCompilation { get; private set; }

        public DesignTimeAspectPipelineFactory? Factory { get; }

        public bool MustReportPausedPipelineAsErrors => this.Factory == null || !this.Factory.IsUserInterfaceAttached;

        protected override void Dispose( bool disposing )
        {
            base.Dispose( disposing );
            this._fileSystemWatcher?.Dispose();
        }

        private CompilationChanges InvalidateCache( Compilation compilation, bool invalidateCompilationResult, CancellationToken cancellationToken )
        {
            this.SetState( this._currentState.InvalidateCacheForNewCompilation( compilation, invalidateCompilationResult, cancellationToken ) );

            return this._currentState.UnprocessedChanges.AssertNotNull();
        }

        public void InvalidateCache()
        {
            using ( this.WithLock() )
            {
                this.SetState( this._currentState.Reset() );
            }
        }

        private bool TryExecutePartial(
            PartialCompilation partialCompilation,
            CancellationToken cancellationToken,
            [NotNullWhen( true )] out CompilationResult? compilationResult )
        {
            var state = this._currentState;

            if ( !PipelineState.TryExecute( ref state, partialCompilation, cancellationToken, out compilationResult ) )
            {
                return false;
            }

            // Intentionally updating the state atomically after successful execution of the method, so the state is
            // not affected by a cancellation.
            this.SetState( state );

            return true;
        }

        public bool TryExecute( Compilation compilation, CancellationToken cancellationToken, [NotNullWhen( true )] out CompilationResult? compilationResult )
        {
            if ( this._compilationResultCache.TryGetValue( compilation, out compilationResult ) )
            {
                return true;
            }

            this.LastCompilation = compilation;

            using ( this.WithLock() )
            {
                if ( this._compilationResultCache.TryGetValue( compilation, out compilationResult ) )
                {
                    return true;
                }

                // Invalidate the cache for the new compilation.
                Compilation? compilationToAnalyze = null;

                if ( this.Status != DesignTimeAspectPipelineStatus.Paused )
                {
                    var changes = this.InvalidateCache( compilation, this.Status != DesignTimeAspectPipelineStatus.Paused, cancellationToken );

                    compilationToAnalyze = changes.CompilationToAnalyze;

                    if ( this.Logger.Trace != null )
                    {
                        if ( compilationToAnalyze != compilation )
                        {
                            this.Logger.Trace?.Log(
                                $"Cache hit: the original compilation is {DebuggingHelper.GetObjectId( compilation )}, but we will analyze the cached compilation {DebuggingHelper.GetObjectId( compilationToAnalyze )}" );
                        }
                    }
                }
                else
                {
                    // If the pipeline is paused, there is no need to track changes because the pipeline will be fully invalidated anyway
                    // when it will be resumed.
                }

                if ( this.Status != DesignTimeAspectPipelineStatus.Paused )
                {
                    PartialCompilation? partialCompilation;

                    if ( this.Status == DesignTimeAspectPipelineStatus.Default )
                    {
                        partialCompilation = PartialCompilation.CreateComplete( compilationToAnalyze! );
                    }
                    else
                    {
                        var dirtySyntaxTrees = this.GetDirtySyntaxTrees( compilationToAnalyze! );

                        if ( dirtySyntaxTrees.Count == 0 )
                        {
                            this.Logger.Trace?.Log( "There is no dirty tree." );
                            partialCompilation = null;
                        }
                        else
                        {
                            partialCompilation = PartialCompilation.CreatePartial( compilationToAnalyze!, dirtySyntaxTrees );
                        }
                    }

                    // Execute the pipeline if required, and update the cache.
                    if ( partialCompilation != null )
                    {
                        Interlocked.Increment( ref this._pipelineExecutionCount );

                        if ( !this.TryExecutePartial( partialCompilation, cancellationToken, out compilationResult ) )
                        {
                            return false;
                        }
                    }

                    // Return the result from the cache.
                    compilationResult = new CompilationResult( this._currentState.PipelineResult, this._currentState.ValidationResult );

                    this._compilationResultCache.Add( compilation, compilationResult );

                    return true;
                }
                else
                {
                    this.Logger.Trace?.Log(
                        $"DesignTimeAspectPipelineCache.TryExecute('{compilation.AssemblyName}', CompilationId = {DebuggingHelper.GetObjectId( compilation )}): "
                        + $"the pipeline is paused, returning from cache only." );

                    // If the pipeline is paused, we only serve pipeline results from the cache.
                    // For validation results, we need to continuously run the templating validators (not the user ones) because the user is likely editing the
                    // template right now. We run only the system validators. We don't run the user validators because of performance -- at this point, we don't have
                    // caching, so we need to validate all syntax trees. If we want to improve performance, we would have to cache system validators separately from the pipeline.

                    var validationResult = this.ValidateWithPausedPipeline( compilation, this, cancellationToken );
                    compilationResult = new CompilationResult( this._currentState.PipelineResult, validationResult );

                    return true;
                }
            }
        }

        private CompilationValidationResult ValidateWithPausedPipeline(
            Compilation compilation,
            DesignTimeAspectPipeline pipeline,
            CancellationToken cancellationToken )
        {
            var resultBuilder = ImmutableDictionary.CreateBuilder<string, SyntaxTreeValidationResult>();
            var diagnostics = new List<Diagnostic>();

            foreach ( var syntaxTree in compilation.SyntaxTrees )
            {
                diagnostics.Clear();

                var semanticModel = compilation.GetSemanticModel( syntaxTree );

                var pipelineMustReportPausedPipelineAsErrors =
                    pipeline.MustReportPausedPipelineAsErrors && pipeline.IsCompileTimeSyntaxTreeOutdated( syntaxTree.FilePath );

                if ( pipelineMustReportPausedPipelineAsErrors )
                {
                    this.Logger.Trace?.Log( $"The syntax tree '{syntaxTree.FilePath}' is marked as outdated." );
                }

                TemplatingCodeValidator.Validate(
                    pipeline.ServiceProvider,
                    semanticModel,
                    diagnostics.Add,
                    pipelineMustReportPausedPipelineAsErrors,
                    true,
                    cancellationToken );

                ImmutableArray<CacheableScopedSuppression> suppressions;

                // Take the cached suppressions so we don't submerge the user with warnings (although these are only validation suppressions, not aspect suppressions).
                if ( this._currentState.ValidationResult.SyntaxTreeResults.TryGetValue( syntaxTree.FilePath, out var syntaxTreeResult ) )
                {
                    suppressions = syntaxTreeResult.Suppressions;
                }
                else
                {
                    suppressions = ImmutableArray<CacheableScopedSuppression>.Empty;
                }

                if ( diagnostics.Count > 0 || !suppressions.IsEmpty )
                {
                    resultBuilder[syntaxTree.FilePath] = new SyntaxTreeValidationResult( syntaxTree, diagnostics.ToImmutableArray(), suppressions );
                }
            }

            return new CompilationValidationResult( resultBuilder.ToImmutable(), DesignTimeValidatorCollectionEqualityKey.Empty );
        }

        private List<SyntaxTree> GetDirtySyntaxTrees( Compilation compilation )
        {
            // Computes the set of semantic models that need to be processed.

            List<SyntaxTree> uncachedSyntaxTrees = new();

            foreach ( var syntaxTree in compilation.SyntaxTrees )
            {
                if ( syntaxTree.FilePath.StartsWith( _sourceGeneratorAssemblyName, StringComparison.Ordinal ) )
                {
                    // This is our own generated file. Don't include.
                    continue;
                }

                if ( this._currentState.PipelineResult.IsSyntaxTreeDirty( syntaxTree ) )
                {
                    uncachedSyntaxTrees.Add( syntaxTree );
                }
            }

            return uncachedSyntaxTrees;
        }

        /// <summary>
        /// Determines whether a compile-time syntax tree is outdated. This happens when the syntax
        /// tree has changed compared to the cached configuration of this pipeline. This method is used to
        /// determine whether an error must displayed in the editor.  
        /// </summary>
        public bool IsCompileTimeSyntaxTreeOutdated( string name )
            => this._currentState.CompileTimeSyntaxTrees is { } compileTimeSyntaxTrees && compileTimeSyntaxTrees.TryGetValue( name, out var syntaxTree )
                                                                                       && syntaxTree == null;

        internal IEnumerable<AspectClass> GetEligibleAspects( Compilation compilation, ISymbol symbol, CancellationToken cancellationToken )
        {
            var classes = this.AspectClasses;

            if ( classes == null )
            {
                yield break;
            }

            IDeclaration? declaration = null;

            foreach ( var aspectClass in classes )
            {
                cancellationToken.ThrowIfCancellationRequested();

                if ( !aspectClass.IsAbstract && aspectClass.IsEligibleFast( symbol ) )
                {
                    // We have a candidate. Create an IDeclaration if we haven't done it yet.
                    if ( declaration == null )
                    {
                        var projectModel = new ProjectModel( compilation, this.ServiceProvider.WithMark( ServiceProviderMark.Project ) );

                        var compilationModel = CompilationModel.CreateInitialInstance(
                            projectModel,
                            PartialCompilation.CreatePartial( compilation, Array.Empty<SyntaxTree>() ) );

                        declaration = compilationModel.Factory.GetDeclaration( symbol );
                    }

                    var eligibleScenarios = aspectClass.GetEligibility( declaration );

                    if ( eligibleScenarios.IncludesAny( EligibleScenarios.All ) )
                    {
                        yield return aspectClass;
                    }
                }
            }
        }

        private Lock WithLock()
        {
            if ( !Monitor.TryEnter( this._sync ) )
            {
                this.Logger.Trace?.Log( $"Waiting for lock on '{this.ProjectOptions.ProjectId}'." );

                Monitor.Enter( this._sync );
            }

            this.Logger.Trace?.Log( $"Lock on '{this.ProjectOptions.ProjectId}' acquired." );

            return new Lock( this );
        }

        private readonly struct Lock : IDisposable
        {
            private readonly DesignTimeAspectPipeline _parent;

            public Lock( DesignTimeAspectPipeline sync )
            {
                this._parent = sync;
            }

            public void Dispose()
            {
                this._parent.Logger.Trace?.Log( $"Releasing lock on '{this._parent.ProjectOptions.ProjectId}'." );
                Monitor.Exit( this._parent._sync );
            }
        }

        public void ValidateTemplatingCode( SemanticModel semanticModel, Action<Diagnostic> addDiagnostic )
        {
            TemplatingCodeValidator.Validate(
                this.ServiceProvider,
                semanticModel,
                addDiagnostic,
                this.IsCompileTimeSyntaxTreeOutdated( semanticModel.SyntaxTree.FilePath ),
                true,
                CancellationToken.None );
        }

        public bool TryApplyAspectToCode(
            string aspectTypeName,
            Compilation inputCompilation,
            ISymbol targetSymbol,
            CancellationToken cancellationToken,
            [NotNullWhen( true )] out PartialCompilation? outputCompilation,
            out ImmutableArray<Diagnostic> diagnostics )
        {
            // Get a compilation _without_ generated code, and map the target symbol.
            var generatedFiles = inputCompilation.SyntaxTrees.Where( SourceGeneratorHelper.IsGeneratedFile );
            var sourceCompilation = inputCompilation.RemoveSyntaxTrees( generatedFiles );

            var sourceSymbol = DocumentationCommentId
                .GetFirstSymbolForDeclarationId( targetSymbol.GetDocumentationCommentId().AssertNotNull(), sourceCompilation )
                .AssertNotNull();

            // TODO: use partial compilation (it does not seem to work).
            var partialCompilation = PartialCompilation.CreateComplete( sourceCompilation );

            DiagnosticList diagnosticList = new();

            if ( !this.TryGetConfiguration( partialCompilation, diagnosticList, true, cancellationToken, out var configuration ) )
            {
                outputCompilation = null;
                diagnostics = diagnosticList.ToImmutableArray();

                return false;
            }

            var result = LiveTemplateAspectPipeline.TryExecute(
                configuration.ServiceProvider,
                this.Domain,
                configuration,
                x => x.AspectClasses.Single( c => c.FullName == aspectTypeName ),
                partialCompilation,
                sourceSymbol,
                diagnosticList,
                cancellationToken,
                out outputCompilation );

            diagnostics = diagnosticList.ToImmutableArray();

            return result;
        }
    }
}