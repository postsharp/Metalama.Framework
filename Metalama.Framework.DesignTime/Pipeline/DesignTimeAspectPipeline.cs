// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Utilities;
using Metalama.Framework.Code;
using Metalama.Framework.DesignTime.Pipeline.Diff;
using Metalama.Framework.DesignTime.Utilities;
using Metalama.Framework.Eligibility;
using Metalama.Framework.Engine;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Options;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Engine.Pipeline.DesignTime;
using Metalama.Framework.Engine.Pipeline.LiveTemplates;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Utilities.Caching;
using Metalama.Framework.Engine.Utilities.Diagnostics;
using Metalama.Framework.Engine.Utilities.Threading;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Metalama.Framework.DesignTime.Pipeline
{
    /// <summary>
    /// The design-time implementation of <see cref="AspectPipeline"/>.
    /// </summary>
    /// Must be public because of testing.
    internal partial class DesignTimeAspectPipeline : BaseDesignTimeAspectPipeline
    {
        private static readonly string _sourceGeneratorAssemblyName = typeof(DesignTimeAspectPipelineFactory).Assembly.GetName().Name.AssertNotNull();

#pragma warning disable CA1805 // Do not initialize unnecessarily
        private readonly WeakCache<Compilation, FallibleResultWithDiagnostics<CompilationResult>> _compilationResultCache = new();
#pragma warning restore CA1805 // Do not initialize unnecessarily
        private readonly IFileSystemWatcher? _fileSystemWatcher;
        private readonly ProjectKey _projectKey;

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
        private readonly SemaphoreSlim _sync = new( 1 );

        // ReSharper disable once InconsistentlySynchronizedField
        internal DesignTimeAspectPipelineStatus Status => this._currentState.Status;

        internal event Action<DesignTimePipelineStatusChangedEventArgs>? StatusChanged;

        private readonly DesignTimeAspectPipelineFactory _factory;

        public ProjectVersionProvider ProjectVersionProvider { get; }

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
        private IEnumerable<AspectClass>? AspectClasses
        {
            get
            {
                if ( !this._currentState.Configuration.HasValue || !this._currentState.Configuration.Value.IsSuccess )
                {
                    return null;
                }

                return this._currentState.Configuration.Value.Value.AspectClasses.OfType<AspectClass>();
            }
        }

        public DesignTimeAspectPipeline(
            DesignTimeAspectPipelineFactory pipelineFactory,
            IProjectOptions projectOptions,
            Compilation compilation,
            bool isTest ) : this( pipelineFactory, projectOptions, compilation.GetProjectKey(), compilation.References, isTest ) { }

        public DesignTimeAspectPipeline(
            DesignTimeAspectPipelineFactory pipelineFactory,
            IProjectOptions projectOptions,
            ProjectKey projectKey,
            IEnumerable<MetadataReference> metadataReferences,
            bool isTest )
            : base(
                pipelineFactory.ServiceProvider.WithService( projectOptions ).WithProjectScopedServices( metadataReferences ),
                isTest,
                pipelineFactory.Domain )
        {
            this._projectKey = projectKey;
            this._factory = pipelineFactory;
            this.ProjectVersionProvider = this.ServiceProvider.GetRequiredService<ProjectVersionProvider>();
            this.Observer = this.ServiceProvider.GetService<IDesignTimeAspectPipelineObserver>();

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

            if ( watchedDirectory != null )
            {
                var fileSystemWatcherFactory = this.ServiceProvider.GetService<IFileSystemWatcherFactory>() ?? new FileSystemWatcherFactory();
                this._fileSystemWatcher = fileSystemWatcherFactory.Create( watchedDirectory, watchedFilter );
                this._fileSystemWatcher.IncludeSubdirectories = false;

                this._fileSystemWatcher.Changed += this.OnOutputDirectoryChanged;
                this._fileSystemWatcher.EnableRaisingEvents = true;
            }
        }

        internal IDesignTimeAspectPipelineObserver? Observer { get; }

        public event EventHandler? PipelineResumed;

        private void OnOutputDirectoryChanged( object sender, FileSystemEventArgs e )
        {
            if ( e.FullPath != this.ProjectOptions.BuildTouchFile || this.Status != DesignTimeAspectPipelineStatus.Paused )
            {
                return;
            }

            // There was an external build. Touch the files to re-run the analyzer.
            this.Logger.Trace?.Log( $"Detected an external build for project '{this._projectKey}'." );

            _ = this.ResumeAsync( true, CancellationToken.None );
        }

        public async ValueTask ResumeAsync( bool touchFiles, CancellationToken cancellationToken )
        {
            using ( await this.WithLock( cancellationToken ) )
            {
                if ( this.Status != DesignTimeAspectPipelineStatus.Paused )
                {
                    this.Logger.Trace?.Log( $"A Resume request was requested for project '{this._projectKey}', but the pipeline was not paused." );

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
                        $"Resuming the pipeline for project '{this._projectKey}'. The following files had compile-time changes: {string.Join( ", ", filesToTouch.Select( x => $"'{x}'" ) )} " );

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
                    this.Logger.Trace?.Log( $"A Resume request was requested for project '{this._projectKey}', but there was no relevant change." );
                }
            }
        }

        internal async ValueTask<FallibleResultWithDiagnostics<AspectPipelineConfiguration>> GetConfigurationAsync(
            PartialCompilation compilation,
            bool ignoreStatus,
            CancellationToken cancellationToken )
        {
            using ( await this.WithLock( cancellationToken ) )
            {
                var state = this._currentState;

                var getConfigurationResult = PipelineState.GetConfiguration(
                    ref state,
                    compilation,
                    ignoreStatus,
                    cancellationToken );

                this.SetState( state );

                return getConfigurationResult;
            }
        }

        public Compilation? LastCompilation { get; private set; }

        public bool MustReportPausedPipelineAsErrors => !this._factory.IsUserInterfaceAttached;

        protected override void Dispose( bool disposing )
        {
            base.Dispose( disposing );
            this._fileSystemWatcher?.Dispose();
            this._sync.Dispose();
        }

        private async ValueTask<ProjectVersion> InvalidateCacheAsync(
            Compilation compilation,
            bool invalidateCompilationResult,
            CancellationToken cancellationToken )
        {
            var newState = await this._currentState.InvalidateCacheForNewCompilationAsync(
                compilation,
                invalidateCompilationResult,
                cancellationToken );

            this.SetState( newState );

            return newState.CompilationVersion.AssertNotNull();
        }

        public async ValueTask InvalidateCacheAsync( CancellationToken cancellationToken )
        {
            using ( await this.WithLock( cancellationToken ) )
            {
                this.SetState( this._currentState.Reset() );
            }
        }

        private async Task<FallibleResultWithDiagnostics<CompilationResult>> ExecutePartialAsync(
            PartialCompilation partialCompilation,
            DesignTimeProjectVersion projectVersion,
            CancellationToken cancellationToken )
        {
            var result = await PipelineState.ExecuteAsync( this._currentState, partialCompilation, projectVersion, cancellationToken );

            // Intentionally updating the state atomically after execution of the method, so the state is
            // not affected by a cancellation.
            this.SetState( result.NewState );

            if ( !result.CompilationResult.IsSuccess )
            {
                return FallibleResultWithDiagnostics<CompilationResult>.Failed( result.CompilationResult.Diagnostics );
            }
            else
            {
                return FallibleResultWithDiagnostics<CompilationResult>.Succeeded( result.CompilationResult.Value, result.CompilationResult.Diagnostics );
            }
        }

        // This method is for testing only.
        public bool TryExecute( Compilation compilation, CancellationToken cancellationToken, [NotNullWhen( true )] out CompilationResult? compilationResult )
        {
            var result = TaskHelper.RunAndWait(
                () => this.ExecuteAsync( compilation, cancellationToken ),
                cancellationToken );

            if ( !result.IsSuccess )
            {
                compilationResult = null;

                return false;
            }
            else
            {
                compilationResult = result.Value;

                return true;
            }
        }

        private async ValueTask<DesignTimeProjectVersion?> GetDesignTimeProjectVersionAsync(
            Compilation compilation,
            CancellationToken cancellationToken )
        {
            var compilationVersion = await this.ProjectVersionProvider.GetCompilationVersionAsync(
                this._currentState.CompilationVersion?.Compilation,
                compilation,
                cancellationToken );

            List<DesignTimeProjectReference> compilationReferences = new();

            foreach ( var reference in compilationVersion.ReferencedProjectVersions.Values )
            {
                var factory = this._factory.AssertNotNull();

                if ( factory.IsMetalamaEnabled( reference.Compilation ) )
                {
                    // This is a Metalama reference. We need to compile the dependency.
                    var referenceResult = await factory.ExecuteAsync( reference.Compilation, cancellationToken );

                    if ( !referenceResult.IsSuccess )
                    {
                        return null;
                    }

                    compilationReferences.Add(
                        new DesignTimeProjectReference(
                            referenceResult.Value.ProjectVersion,
                            referenceResult.Value.TransformationResult ) );
                }
                else
                {
                    // It is a non-Metalama reference.
                    var projectKey = reference.Compilation.GetProjectKey();
                    var projectTracker = factory.GetNonMetalamaProjectTracker( projectKey );

                    if ( this._currentState.CompilationVersion?.ReferencedProjectVersions == null
                         || this._currentState.CompilationVersion.ReferencedProjectVersions.TryGetValue(
                             projectKey,
                             out var oldReference ) )
                    {
                        oldReference = null;
                    }

                    var compilationReference = await projectTracker.GetCompilationReferenceAsync(
                        oldReference?.Compilation,
                        reference.Compilation,
                        cancellationToken );

                    compilationReferences.Add( compilationReference );
                }
            }

            return new DesignTimeProjectVersion( compilationVersion, compilationReferences );
        }

        public async ValueTask<FallibleResultWithDiagnostics<CompilationResult>> ExecuteAsync(
            Compilation compilation,
            CancellationToken cancellationToken )
        {
            if ( this._compilationResultCache.TryGetValue( compilation, out var compilationResult ) )
            {
                return compilationResult;
            }

            this.LastCompilation = compilation;

            using ( await this.WithLock( cancellationToken ) )
            {
                if ( this._compilationResultCache.TryGetValue( compilation, out compilationResult ) )
                {
                    return compilationResult;
                }

                var projectVersion = await this.GetDesignTimeProjectVersionAsync( compilation, cancellationToken );

                if ( projectVersion == null )
                {
                    // A dependency could not be compiled.
                    return default;
                }

                // Invalidate the cache for the new compilation.
                Compilation? compilationToAnalyze = null;

                if ( this.Status != DesignTimeAspectPipelineStatus.Paused )
                {
                    var compilationVersion = await this.InvalidateCacheAsync(
                        compilation,
                        true,
                        cancellationToken );

                    compilationToAnalyze = compilationVersion.CompilationToAnalyze;

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

                        var executionResult = await this.ExecutePartialAsync( partialCompilation, projectVersion, cancellationToken );

                        if ( !executionResult.IsSuccess )
                        {
                            compilationResult = FallibleResultWithDiagnostics<CompilationResult>.Failed( executionResult.Diagnostics );

                            if ( !this._compilationResultCache.TryAdd( compilation, compilationResult ) )
                            {
                                throw new AssertionFailedException();
                            }

                            return compilationResult;
                        }
                    }

                    // Return the result from the cache.
                    compilationResult = new CompilationResult(
                        this._currentState.CompilationVersion.AssertNotNull(),
                        this._currentState.PipelineResult,
                        this._currentState.ValidationResult,
                        this._currentState.Configuration!.Value.Value.CompileTimeProject );

                    if ( !this._compilationResultCache.TryAdd( compilation, compilationResult ) )
                    {
                        throw new AssertionFailedException();
                    }

                    return compilationResult;
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

                    compilationResult = new CompilationResult(
                        this._currentState.CompilationVersion.AssertNotNull(),
                        this._currentState.PipelineResult,
                        validationResult,
                        this._currentState.Configuration!.Value.Value.CompileTimeProject );

                    return compilationResult;
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

        private async ValueTask<Lock> WithLock( CancellationToken cancellationToken )
        {
            if ( this._sync.CurrentCount < 1 )
            {
                this.Logger.Trace?.Log( $"Waiting for lock on '{this._projectKey}'." );
            }

            await this._sync.WaitAsync( cancellationToken );

            this.Logger.Trace?.Log( $"Lock on '{this._projectKey}' acquired." );

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
                this._parent.Logger.Trace?.Log( $"Releasing lock on '{this._parent._projectKey}'." );
                this._parent._sync.Release();
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

        public async Task<(bool Success, PartialCompilation? Compilation, ImmutableArray<Diagnostic> Diagnostics)> ApplyAspectToCodeAsync(
            string aspectTypeName,
            Compilation inputCompilation,
            ISymbol targetSymbol,
            CancellationToken cancellationToken )
        {
            // Get a compilation _without_ generated code, and map the target symbol.
            var generatedFiles = inputCompilation.SyntaxTrees.Where( SourceGeneratorHelper.IsGeneratedFile );
            var sourceCompilation = inputCompilation.RemoveSyntaxTrees( generatedFiles );

            var sourceSymbol = DocumentationCommentId
                .GetFirstSymbolForDeclarationId( targetSymbol.GetDocumentationCommentId().AssertNotNull(), sourceCompilation )
                .AssertNotNull();

            // TODO: use partial compilation (it does not seem to work).
            var partialCompilation = PartialCompilation.CreateComplete( sourceCompilation );

            DiagnosticBag diagnosticBag = new();

            var getConfigurationResult = await this.GetConfigurationAsync( partialCompilation, true, cancellationToken );

            if ( !getConfigurationResult.IsSuccess )
            {
                return (false, null, diagnosticBag.ToImmutableArray());
            }

            var configuration = getConfigurationResult.Value;

            var result = await LiveTemplateAspectPipeline.ExecuteAsync(
                configuration.ServiceProvider,
                this.Domain,
                configuration,
                x => x.AspectClasses.Single( c => c.FullName == aspectTypeName ),
                partialCompilation,
                sourceSymbol,
                diagnosticBag,
                cancellationToken );

            if ( !result.IsSuccess )
            {
                return (false, null, diagnosticBag.ToImmutableArray());
            }
            else
            {
                return (true, result.Value, diagnosticBag.ToImmutableArray());
            }
        }
    }
}