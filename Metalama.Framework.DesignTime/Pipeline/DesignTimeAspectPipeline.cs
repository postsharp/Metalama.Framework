﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Licensing.Consumption;
using Metalama.Backstage.Utilities;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.DesignTime.Pipeline.Diff;
using Metalama.Framework.DesignTime.Rpc;
using Metalama.Framework.DesignTime.Rpc.Notifications;
using Metalama.Framework.DesignTime.Utilities;
using Metalama.Framework.Eligibility;
using Metalama.Framework.Engine;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Licensing;
using Metalama.Framework.Engine.Options;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Engine.Pipeline.DesignTime;
using Metalama.Framework.Engine.Pipeline.LiveTemplates;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Utilities.Caching;
using Metalama.Framework.Engine.Utilities.Diagnostics;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Metalama.Framework.Engine.Utilities.Threading;
using Metalama.Framework.Services;
using Microsoft.CodeAnalysis;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Metalama.Framework.DesignTime.Pipeline;

/// <summary>
/// The design-time implementation of <see cref="AspectPipeline"/>.
/// </summary>
/// Must be public because of testing.
internal sealed partial class DesignTimeAspectPipeline : BaseDesignTimeAspectPipeline
{
    private static readonly string _sourceGeneratorAssemblyName = typeof(DesignTimeAspectPipelineFactory).Assembly.GetName().Name.AssertNotNull();

    private readonly WeakCache<Compilation, FallibleResultWithDiagnostics<CompilationResult>> _compilationResultCache = new();
    private readonly IFileSystemWatcher? _fileSystemWatcher;
    private readonly ConcurrentQueue<Func<ValueTask>> _jobQueue = new();
    private readonly IDesignTimeAspectPipelineObserver? _observer;
    private readonly SemaphoreSlim _sync = new( 1 );
    private readonly DesignTimeAspectPipelineFactory _factory;
    private readonly AnalysisProcessEventHub _eventHub;

    private bool _mustProcessQueue;

    public ProjectKey ProjectKey { get; }

    // This field should not be changed directly, but only through the SetState method.
    private PipelineState _currentState;

    private int _pipelineExecutionCount;

    /// <summary>
    /// Gets the number of times the pipeline has been executed. Useful for testing purposes.
    /// </summary>
    public int PipelineExecutionCount => this._pipelineExecutionCount;

    // ReSharper disable once InconsistentlySynchronizedField
    internal DesignTimeAspectPipelineStatus Status => this._currentState.Status;

    public ProjectVersionProvider ProjectVersionProvider { get; }

    public long SnapshotId => this._currentState.SnapshotId;

    public DesignTimeAspectPipeline(
        DesignTimeAspectPipelineFactory pipelineFactory,
        IProjectOptions projectOptions,
        Compilation compilation ) : this( pipelineFactory, projectOptions, compilation.GetProjectKey(), compilation.References ) { }

    public DesignTimeAspectPipeline(
        DesignTimeAspectPipelineFactory pipelineFactory,
        IProjectOptions projectOptions,
        ProjectKey projectKey,
        IEnumerable<MetadataReference> metadataReferences )
        : base(
            GetServiceProvider( pipelineFactory.ServiceProvider, projectOptions, metadataReferences ),
            pipelineFactory.Domain )
    {
        this.ProjectKey = projectKey;
        this._factory = pipelineFactory;
        this.ProjectVersionProvider = this.ServiceProvider.Global.GetRequiredService<ProjectVersionProvider>();
        this._observer = this.ServiceProvider.GetService<IDesignTimeAspectPipelineObserver>();
        this._eventHub = this.ServiceProvider.Global.GetRequiredService<AnalysisProcessEventHub>();
        this._eventHub.CompilationResultChanged += this.OnOtherPipelineCompilationResultChanged;
        this._eventHub.PipelineStatusChangedEvent.RegisterHandler( this.OnOtherPipelineStatusChangedAsync );

        this._currentState = new PipelineState( this );

        // The design-time pipeline contains project-scoped services for performance reasons: the pipeline may be called several
        // times with the same compilation.

        if ( string.IsNullOrEmpty( this.ProjectOptions.BuildTouchFile ) )
        {
            return;
        }

        this.Logger.Trace?.Log( $"BuildTouchFile={this.ProjectOptions.BuildTouchFile}" );

        // Initialize FileSystemWatcher.
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

    private void OnOtherPipelineCompilationResultChanged( CompilationResultChangedEventArgs args )
    {
        if ( this.Status != DesignTimeAspectPipelineStatus.Ready )
        {
            return;
        }

        var dependencies = this._currentState.Dependencies;

        if ( dependencies.IsUninitialized )
        {
            return;
        }

        if ( !dependencies.DependenciesByMasterProject.TryGetValue( args.ProjectKey, out var dependenciesInThisProject ) )
        {
            return;
        }

        this.Logger.Trace?.Log( $"Processing change notification from dependent project '{args.ProjectKey}'." );

        if ( !args.IsPartialCompilation || args.SyntaxTreePaths.Any( p => dependenciesInThisProject.DependenciesByMasterFilePath.ContainsKey( p ) ) )
        {
            this.Logger.Trace?.Log( $"Processing change notification from dependent project '{args.ProjectKey}': the current project may be affected." );
            this._eventHub.OnProjectDirty( this.ProjectKey );
        }
        else
        {
            this.Logger.Trace?.Log( $"Processing change notification from dependent project '{args.ProjectKey}': the current project is not affected." );
        }
    }

    private async Task OnOtherPipelineStatusChangedAsync( DesignTimePipelineStatusChangedEventArgs args )
    {
        if ( args.Pipeline.ProjectKey != this.ProjectKey )
        {
            // TODO PERF: Check that the changed project is actually a dependency of the current project.

            if ( args.IsPausing && this.Status != DesignTimeAspectPipelineStatus.Paused )
            {
                this.Logger.Trace?.Log( $"Pausing '{this.ProjectKey}' because the dependent project '{args.Pipeline.ProjectKey}' has resumed." );

                await this.ExecuteWithLockOrEnqueueAsync( () => this.SetStateAsync( this._currentState.Pause() ) );
            }
            else if ( args.IsResuming && this.Status != DesignTimeAspectPipelineStatus.Default )
            {
                this.Logger.Trace?.Log( $"Resuming '{this.ProjectKey}' because the dependent project '{args.Pipeline.ProjectKey}' has resumed." );
                await this.ResumeAsync( CancellationToken.None );
            }
        }
    }

    private static ServiceProvider<IProjectService> GetServiceProvider(
        ServiceProvider<IGlobalService> serviceProvider,
        IProjectOptions projectOptions,
        IEnumerable<MetadataReference> metadataReferences )
    {
        var projectServiceProvider = serviceProvider.WithProjectScopedServices( projectOptions, metadataReferences );

        if ( !projectOptions.IsTest || !string.IsNullOrEmpty( projectOptions.License ) )
        {
            // We always ignore unattended licenses in a design-time process, but we ignore the user profile licenses only in tests.
            projectServiceProvider = projectServiceProvider.AddLicenseConsumptionManager(
                new LicensingInitializationOptions()
                {
                    ProjectLicense = projectOptions.License, IgnoreUserProfileLicenses = projectOptions.IsTest, IgnoreUnattendedProcessLicense = true
                } );
        }

        return projectServiceProvider;
    }

    private async ValueTask SetStateAsync( PipelineState state )
    {
        var oldStatus = this._currentState.Status;
        this._currentState = state;

        if ( oldStatus != state.Status )
        {
            await this._eventHub.OnPipelineStatusChangedEventAsync( new DesignTimePipelineStatusChangedEventArgs( this, oldStatus, state.Status ) );
        }
    }

    // It's ok if we return an obsolete project in the use cases of this property.
    // ReSharper disable once InconsistentlySynchronizedField
    private IReadOnlyCollection<IAspectClass>? AspectClasses
    {
        get
        {
            if ( this._currentState.Configuration is not { IsSuccessful: true } )
            {
                return null;
            }

            return this._currentState.Configuration.Value.Value.AspectClasses;
        }
    }

#pragma warning disable VSTHRD100
    private async void OnOutputDirectoryChanged( object sender, FileSystemEventArgs e )
    {
        try
        {
            if ( e.FullPath != this.ProjectOptions.BuildTouchFile || this.Status != DesignTimeAspectPipelineStatus.Paused )
            {
                return;
            }

            // There was an external build. Touch the files to re-run the analyzer.
            this.Logger.Trace?.Log( $"Detected an external build for project '{this.ProjectKey}'." );

            await this.ResumeAsync( CancellationToken.None );

            // Raise the event.
            await this._eventHub.OnExternalBuildCompletedEventAsync( this.ProjectKey );
        }
        catch ( Exception exception )
        {
            DesignTimeExceptionHandler.ReportException( exception );
        }
    }
#pragma warning restore VSTHRD100

    public async ValueTask ResumeAsync( CancellationToken cancellationToken )
    {
        this.Logger.Trace?.Log( $"Resuming the pipeline for project '{this.ProjectKey}'." );

        using ( await this.WithLockAsync( cancellationToken ) )
        {
            try
            {
                if ( this.Status != DesignTimeAspectPipelineStatus.Paused )
                {
                    this.Logger.Trace?.Log( $"A Resume request was requested for project '{this.ProjectKey}', but the pipeline was not paused." );

                    return;
                }

                // Touch the modified compile-time files so that they are analyzed again by Roslyn, and our "edit in progress" diagnostic
                // is removed.
                if ( this.MustReportPausedPipelineAsErrors )
                {
                    foreach ( var file in this._currentState.CompileTimeSyntaxTrees.AssertNotNull() )
                    {
                        if ( file.Value == null )
                        {
                            this.Logger.Trace?.Log( $"Touching file '{file.Key}'." );

                            RetryHelper.Retry(
                                () =>
                                {
                                    if ( File.Exists( file.Key ) )
                                    {
                                        File.SetLastWriteTimeUtc( file.Key, DateTime.UtcNow );
                                    }
                                },
                                logger: this.Logger );
                        }
                    }
                }

                // Reset the pipeline.
                await this.SetStateAsync( this._currentState.Reset() );

                // Notify that the pipeline must be executed again.
                this._eventHub.OnProjectDirty( this.ProjectKey );
            }
            finally
            {
                await this.ProcessJobQueueAsync();
            }
        }
    }

    internal async ValueTask<FallibleResultWithDiagnostics<AspectPipelineConfiguration>> GetConfigurationAsync(
        PartialCompilation compilation,
        bool ignoreStatus,
        TestableCancellationToken cancellationToken )
    {
        using ( await this.WithLockAsync( cancellationToken ) )
        {
            var state = this._currentState;

            var getConfigurationResult = PipelineState.GetConfiguration(
                ref state,
                compilation,
                ignoreStatus,
                cancellationToken );

            await this.SetStateAsync( state );

            await this.ProcessJobQueueAsync();

            return getConfigurationResult;
        }
    }

    public bool MustReportPausedPipelineAsErrors => !this._eventHub.IsUserInterfaceAttached;

    protected override void Dispose( bool disposing )
    {
        base.Dispose( disposing );
        this._fileSystemWatcher?.Dispose();
        this._sync.Dispose();
        this._eventHub.PipelineStatusChangedEvent.UnregisterHandler( this.OnOtherPipelineStatusChangedAsync );
        this._eventHub.CompilationResultChanged -= this.OnOtherPipelineCompilationResultChanged;
    }

    internal async ValueTask<ProjectVersion> InvalidateCacheAsync(
        Compilation compilation,
        TestableCancellationToken cancellationToken )
    {
        var newState = await this._currentState.InvalidateCacheForNewCompilationAsync(
            compilation,
            true,
            cancellationToken );

        await this.SetStateAsync( newState );

        return newState.ProjectVersion.AssertNotNull();
    }

    public async ValueTask ResetCacheAsync( CancellationToken cancellationToken )
    {
        using ( await this.WithLockAsync( cancellationToken ) )
        {
            await this.SetStateAsync( this._currentState.Reset() );

            this._eventHub.OnProjectDirty( this.ProjectKey );

            await this.ProcessJobQueueAsync();
        }
    }

    private async Task<FallibleResultWithDiagnostics<CompilationResult>> ExecutePartialAsync(
        PartialCompilation partialCompilation,
        DesignTimeProjectVersion projectVersion,
        TestableCancellationToken cancellationToken )
    {
        var result = await PipelineState.ExecuteAsync( this._currentState, partialCompilation, projectVersion, cancellationToken );

        // Intentionally updating the state atomically after execution of the method, so the state is
        // not affected by a cancellation.
        await this.SetStateAsync( result.NewState );

        if ( !result.CompilationResult.IsSuccessful )
        {
            return FallibleResultWithDiagnostics<CompilationResult>.Failed( result.CompilationResult.Diagnostics );
        }
        else
        {
            return FallibleResultWithDiagnostics<CompilationResult>.Succeeded( result.CompilationResult.Value, result.CompilationResult.Diagnostics );
        }
    }

    public FallibleResultWithDiagnostics<CompilationResult> Execute( Compilation compilation, TestableCancellationToken cancellationToken = default )
        => TaskHelper.RunAndWait( () => this.ExecuteAsync( compilation, cancellationToken ), cancellationToken );

    // This method is for testing only.
    public bool TryExecute(
        Compilation compilation,
        TestableCancellationToken cancellationToken,
        [NotNullWhen( true )] out CompilationResult? compilationResult )
    {
        var result = TaskHelper.RunAndWait(
            () => this.ExecuteAsync( compilation, cancellationToken ),
            cancellationToken );

        if ( !result.IsSuccessful )
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

    internal async ValueTask<FallibleResultWithDiagnostics<DesignTimeProjectVersion>> GetDesignTimeProjectVersionAsync(
        Compilation compilation,
        TestableCancellationToken cancellationToken )
    {
        var pipelineStatus = this.Status;

        var compilationVersion = await this.ProjectVersionProvider.GetCompilationVersionAsync(
            this._currentState.ProjectVersion?.Compilation,
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

                if ( !referenceResult.IsSuccessful )
                {
                    return FallibleResultWithDiagnostics<DesignTimeProjectVersion>.Failed( referenceResult.Diagnostics );
                }

                compilationReferences.Add(
                    new DesignTimeProjectReference(
                        referenceResult.Value.ProjectVersion,
                        referenceResult.Value.TransformationResult ) );

                if ( referenceResult.Value.PipelineStatus == DesignTimeAspectPipelineStatus.Paused )
                {
                    pipelineStatus = DesignTimeAspectPipelineStatus.Paused;
                }
            }
            else
            {
                // It is a non-Metalama reference.
                var projectKey = reference.Compilation.GetProjectKey();
                var projectTracker = factory.GetNonMetalamaProjectTracker( projectKey );

                if ( this._currentState.ProjectVersion?.ReferencedProjectVersions == null
                     || this._currentState.ProjectVersion.ReferencedProjectVersions.TryGetValue(
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

        return new DesignTimeProjectVersion( compilationVersion, compilationReferences, pipelineStatus );
    }

    public async ValueTask<FallibleResultWithDiagnostics<CompilationResult>> ExecuteAsync(
        Compilation compilation,
        TestableCancellationToken cancellationToken = default )
    {
        if ( this._compilationResultCache.TryGetValue( compilation, out var compilationResult ) )
        {
            if ( !compilationResult.IsSuccessful )
            {
                this.Logger.Trace?.Log( "Returning a cached but failed compilation." );
            }

            return compilationResult;
        }

        try
        {
            using ( await this.WithLockAsync( cancellationToken ) )
            {
                try
                {
                    if ( this._compilationResultCache.TryGetValue( compilation, out compilationResult ) )
                    {
                        return compilationResult;
                    }

                    var projectVersion = await this.GetDesignTimeProjectVersionAsync( compilation, cancellationToken );

                    if ( !projectVersion.IsSuccessful )
                    {
                        // A dependency could not be compiled.
                        return FallibleResultWithDiagnostics<CompilationResult>.Failed( projectVersion.Diagnostics );
                    }

                    // If a dependency project was paused, we must pause too.
                    if ( this.Status != DesignTimeAspectPipelineStatus.Paused
                         && projectVersion.Value.PipelineStatus == DesignTimeAspectPipelineStatus.Paused )
                    {
                        await this.SetStateAsync( this._currentState.Pause() );
                    }

                    Compilation? compilationToAnalyze = null;

                    if ( this.Status != DesignTimeAspectPipelineStatus.Paused )
                    {
                        // Invalidate the cache for the new compilation.
                        var compilationVersion = await this.InvalidateCacheAsync(
                            compilation,
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

                            var executionResult = await this.ExecutePartialAsync( partialCompilation, projectVersion.Value, cancellationToken );

                            if ( !executionResult.IsSuccessful )
                            {
                                compilationResult = FallibleResultWithDiagnostics<CompilationResult>.Failed( executionResult.Diagnostics );

                                if ( !this._compilationResultCache.TryAdd( compilation, compilationResult ) )
                                {
                                    throw new AssertionFailedException( $"Results of compilation '{this.ProjectKey}' were already in the cache." );
                                }

                                return compilationResult;
                            }

                            // Publish a change notification.
                            var notification = new CompilationResultChangedEventArgs(
                                this.ProjectKey,
                                partialCompilation.IsPartial,
                                partialCompilation.IsPartial ? partialCompilation.SyntaxTrees.SelectAsImmutableArray( t => t.Key ) : default );

                            this._eventHub.PublishCompilationResultChangedNotification( notification );
                        }

                        // Return the result from the cache.
                        compilationResult = new CompilationResult(
                            this._currentState.ProjectVersion.AssertNotNull(),
                            this._currentState.PipelineResult,
                            this._currentState.ValidationResult,
                            this._currentState.Status );

                        if ( !this._compilationResultCache.TryAdd( compilation, compilationResult ) )
                        {
                            throw new AssertionFailedException( $"Results of compilation '{this.ProjectKey}' were already in the cache." );
                        }

                        return compilationResult;
                    }
                    else
                    {
                        this.Logger.Trace?.Log(
                            $"DesignTimeAspectPipeline.ExecuteAsync('{compilation.AssemblyName}', CompilationId = {DebuggingHelper.GetObjectId( compilation )}): "
                            + $"the pipeline is paused, returning from cache only." );

                        // If the pipeline is paused, we only serve pipeline results from the cache.
                        // For validation results, we need to continuously run the templating validators (not the user ones) because the user is likely editing the
                        // template right now. We run only the system validators. We don't run the user validators because of performance -- at this point, we don't have
                        // caching, so we need to validate all syntax trees. If we want to improve performance, we would have to cache system validators separately from the pipeline.

                        var compilationContext = this.ServiceProvider.GetRequiredService<CompilationContextFactory>().GetInstance( compilation );

                        var validationResult = this.ValidateWithPausedPipeline( this.ServiceProvider, compilationContext, this, cancellationToken );

                        if ( this._currentState.ProjectVersion != null )
                        {
                            compilationResult = new CompilationResult(
                                this._currentState.ProjectVersion.AssertNotNull(),
                                this._currentState.PipelineResult,
                                validationResult,
                                this._currentState.Status );
                        }
                        else
                        {
                            // The pipeline was paused before being first executed.
                            compilationResult = FallibleResultWithDiagnostics<CompilationResult>.Failed( ImmutableArray<Diagnostic>.Empty );
                        }

                        return compilationResult;
                    }
                }
                finally
                {
                    await this.ProcessJobQueueAsync();
                }
            }
        }
        catch ( OperationCanceledException )
        {
            this.Logger.Warning?.Log(
                $"DesignTimeAspectPipeline.ExecuteAsync('{compilation.AssemblyName}', CompilationId = {DebuggingHelper.GetObjectId( compilation )}): cancelled." );

            throw;
        }
    }

    private CompilationValidationResult ValidateWithPausedPipeline(
        ProjectServiceProvider serviceProvider,
        CompilationContext compilationContext,
        DesignTimeAspectPipeline pipeline,
        CancellationToken cancellationToken )
    {
        var resultBuilder = ImmutableDictionary.CreateBuilder<string, SyntaxTreeValidationResult>();
        var diagnostics = new List<Diagnostic>();
        var semanticModelProvider = compilationContext.SemanticModelProvider;

        foreach ( var syntaxTree in compilationContext.Compilation.SyntaxTrees )
        {
            diagnostics.Clear();

            var semanticModel = semanticModelProvider.GetSemanticModel( syntaxTree );

            var pipelineMustReportPausedPipelineAsErrors =
                pipeline.MustReportPausedPipelineAsErrors && pipeline.IsCompileTimeSyntaxTreeOutdated( syntaxTree.FilePath );

            if ( pipelineMustReportPausedPipelineAsErrors )
            {
                this.Logger.Trace?.Log( $"The syntax tree '{syntaxTree.FilePath}' is marked as outdated." );
            }

            TemplatingCodeValidator.Validate(
                serviceProvider,
                compilationContext,
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
                resultBuilder[syntaxTree.FilePath] = new SyntaxTreeValidationResult( diagnostics.ToImmutableArray(), suppressions );
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

    private List<DesignTimeAspectInstance>? GetAspectInstancesOnSymbol( ISymbol symbol )
    {
        // Check the aspects already on the declaration.
        var filePath = symbol.GetPrimaryDeclaration()?.SyntaxTree.FilePath;

        if ( filePath == null )
        {
            return null;
        }

        var symbolId = symbol.GetSerializableId();

        if ( !this._currentState.PipelineResult.SyntaxTreeResults.TryGetValue( filePath, out var result ) )
        {
            return null;
        }

        return result.AspectInstances.Where( i => i.TargetDeclarationId == symbolId ).ToList();
    }

    internal IEnumerable<AspectClass> GetEligibleAspects( Compilation compilation, ISymbol symbol, TestableCancellationToken cancellationToken )
    {
        var classes = this.AspectClasses;

        if ( classes == null )
        {
            yield break;
        }

        var compilationContext = this.ServiceProvider.GetRequiredService<CompilationContextFactory>().GetInstance( compilation );

        var currentAspectInstances = (IReadOnlyList<DesignTimeAspectInstance>?) this.GetAspectInstancesOnSymbol( symbol )
                                     ?? Array.Empty<DesignTimeAspectInstance>();

        IDeclaration? declaration = null;

        foreach ( var aspectClass in classes.OfType<AspectClass>() )
        {
            cancellationToken.ThrowIfCancellationRequested();

            this.Logger.Trace?.Log( $"Considering the eligibility of aspect '{aspectClass.ShortName}' on '{symbol}'." );

            // Check if there is already an instance of this aspect class on the target.
            if ( currentAspectInstances.Any( i => i.AspectClassFullName == aspectClass.FullName ) )
            {
                this.Logger.Trace?.Log( "The aspect is not eligible because it has already been added to the symbol." );

                continue;
            }

            // Check if the aspect class is accessible from the symbol.

            var aspectClassSymbol = compilationContext.SerializableTypeIdProvider.ResolveId( aspectClass.TypeId );

            if ( !compilation.IsSymbolAccessibleWithin( aspectClassSymbol, (ISymbol?) symbol.GetClosestContainingType() ?? symbol.ContainingAssembly ) )
            {
                this.Logger.Trace?.Log( "The aspect is not eligible because it is not accessible from the symbol." );

                continue;
            }

            if ( !aspectClass.IsAbstract && aspectClass.IsEligibleFast( symbol ) )
            {
                // We have a candidate. Create an IDeclaration if we haven't done it yet.
                if ( declaration == null )
                {
                    var projectModel = new ProjectModel( compilation, this.ServiceProvider );

                    var compilationModel = CompilationModel.CreateInitialInstance(
                        projectModel,
                        PartialCompilation.CreatePartial( compilation, Array.Empty<SyntaxTree>() ),
                        new PipelineResultBasedAspectRepository( this._currentState.PipelineResult ) );

                    declaration = compilationModel.Factory.GetDeclaration( symbol );
                }

                // Filter with eligibility.
                var eligibleScenarios = aspectClass.GetEligibility( declaration );

                if ( eligibleScenarios.IncludesAny( EligibleScenarios.All ) )
                {
                    yield return aspectClass;
                }
            }
        }
    }

    private async ValueTask ExecuteWithLockOrEnqueueAsync( Func<ValueTask> action )
    {
        using ( var @lock = await this.WithLockAsync( 0, CancellationToken.None ) )
        {
            if ( @lock.IsAcquired )
            {
                await action();
                await this.ProcessJobQueueAsync();
            }
            else
            {
                this._jobQueue.Enqueue( action );
            }
        }
    }

    private async ValueTask ProcessJobQueueAsync()
    {
        this._mustProcessQueue = false;

        while ( this._jobQueue.TryDequeue( out var job ) )
        {
            await job();
        }
    }

    private ValueTask<Lock> WithLockAsync( CancellationToken cancellationToken ) => this.WithLockAsync( -1, cancellationToken );

    private async ValueTask<Lock> WithLockAsync( int timeout, CancellationToken cancellationToken )
    {
        if ( this._sync.CurrentCount < 1 )
        {
            this.Logger.Trace?.Log( $"Waiting for lock on '{this.ProjectKey}'." );
        }

        var acquired = await this._sync.WaitAsync( timeout, cancellationToken );

        if ( acquired )
        {
            this._mustProcessQueue = true;
            this.Logger.Trace?.Log( $"Lock on '{this.ProjectKey}' acquired." );
        }
        else
        {
            this.Logger.Trace?.Log( $"Lock on '{this.ProjectKey}' not acquired." );
        }

        return new Lock( this, acquired );
    }

    private readonly struct Lock : IDisposable
    {
        private readonly DesignTimeAspectPipeline _parent;

        public Lock( DesignTimeAspectPipeline sync, bool isAcquired )
        {
            this._parent = sync;
            this.IsAcquired = isAcquired;
        }

        public bool IsAcquired { get; }

        public void Dispose()
        {
            if ( this.IsAcquired )
            {
                if ( this._parent._mustProcessQueue )
                {
                    throw new AssertionFailedException( "Queue not empty." );
                }

                this._parent.Logger.Trace?.Log( $"Releasing lock on '{this._parent.ProjectKey}'." );
                this._parent._sync.Release();
            }
        }
    }

    public void ValidateTemplatingCode( SemanticModel semanticModel, Action<Diagnostic> addDiagnostic )
        => TemplatingCodeValidator.Validate(
            this.ServiceProvider,
            semanticModel,
            addDiagnostic,
            this.IsCompileTimeSyntaxTreeOutdated( semanticModel.SyntaxTree.FilePath ),
            true,
            CancellationToken.None );

    public async Task<(bool Success, PartialCompilation? Compilation, ImmutableArray<Diagnostic> Diagnostics)> ApplyAspectToCodeAsync(
        string aspectTypeName,
        Compilation inputCompilation,
        ISymbol targetSymbol,
        bool isComputingPreview,
        TestableCancellationToken cancellationToken )
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

        if ( !getConfigurationResult.IsSuccessful )
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
            isComputingPreview,
            cancellationToken );

        if ( !result.IsSuccessful )
        {
            return (false, null, diagnosticBag.ToImmutableArray());
        }
        else
        {
            return (true, result.Value, diagnosticBag.ToImmutableArray());
        }
    }

    public CompilationPipelineResult CompilationPipelineResult => this._currentState.PipelineResult;

    public override string ToString() => $"{this.GetType().Name}, Project='{this.ProjectKey}'";
}