﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Licensing.Consumption;
using Metalama.Backstage.Utilities;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.DesignTime.Contracts.EntryPoint;
using Metalama.Framework.DesignTime.Contracts.Pipeline;
using Metalama.Framework.DesignTime.Pipeline.Diff;
using Metalama.Framework.DesignTime.Rpc;
using Metalama.Framework.DesignTime.Rpc.Notifications;
using Metalama.Framework.DesignTime.Services;
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
using System.Diagnostics;
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
    private readonly ConcurrentQueue<Func<AsyncExecutionContext, ValueTask>> _jobQueue = new();
    private readonly IDesignTimeAspectPipelineObserver? _observer;
    private readonly SemaphoreSlim _sync = new( 1 );
    private readonly IDesignTimeEntryPointConsumer? _entryPointConsumer;
    private readonly AnalysisProcessEventHub _eventHub;
    private readonly DesignTimeAspectPipelineFactory _pipelineFactory;
    private readonly ITaskRunner _taskRunner;

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
        this._pipelineFactory = pipelineFactory;
        this._entryPointConsumer = (IDesignTimeEntryPointConsumer?) this.ServiceProvider.Global.Underlying.GetService( typeof(IDesignTimeEntryPointConsumer) );
        this.ProjectVersionProvider = this.ServiceProvider.Global.GetRequiredService<ProjectVersionProvider>();
        this._observer = this.ServiceProvider.GetService<IDesignTimeAspectPipelineObserver>();
        this._eventHub = this.ServiceProvider.Global.GetRequiredService<AnalysisProcessEventHub>();
        this._eventHub.CompilationResultChanged += this.OnOtherPipelineCompilationResultChanged;
        this._eventHub.PipelineStatusChangedEvent.RegisterHandler( this.OnOtherPipelineStatusChangedAsync );
        this._taskRunner = this.ServiceProvider.Global.GetRequiredService<ITaskRunner>();

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

    private Task OnOtherPipelineStatusChangedAsync( DesignTimePipelineStatusChangedEventArgs arg )
    {
        return this.OnOtherPipelineStatusChangedAsync( AsyncExecutionContext.Get( $"{this.ProjectKey}:PipelineStatusChangedEvent" ), arg );
    }

    private async Task OnOtherPipelineStatusChangedAsync( AsyncExecutionContext executionContext, DesignTimePipelineStatusChangedEventArgs args )
    {
        if ( this._currentState.ProjectVersion?.ReferencedProjectVersions.ContainsKey( args.Pipeline.ProjectKey ) == true )
        {
            if ( args.IsPausing && this.Status != DesignTimeAspectPipelineStatus.Paused )
            {
                this.Logger.Trace?.Log( $"Pausing '{this.ProjectKey}' because the dependent project '{args.Pipeline.ProjectKey}' has resumed." );

                await this.ExecuteIfLockAvailableOrEnqueueAsync( context => this.SetStateAsync( this._currentState.Pause(), context ), executionContext );
            }
            else if ( args.IsResuming && this.Status != DesignTimeAspectPipelineStatus.Default )
            {
                this.Logger.Trace?.Log( $"Resuming '{this.ProjectKey}' because the dependent project '{args.Pipeline.ProjectKey}' has resumed." );
                await this.ExecuteIfLockAvailableOrEnqueueAsync( this.ResumeCoreAsync, executionContext );
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

    private async ValueTask SetStateAsync( PipelineState state, AsyncExecutionContext executionContext )
    {
#if DEBUG
        executionContext.RequireObject( this );
#endif

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

            await this.ResumeAsync( AsyncExecutionContext.Get(), CancellationToken.None );

            // Raise the event.
            await this._eventHub.OnExternalBuildCompletedEventAsync( this.ProjectKey );
        }
        catch ( Exception exception )
        {
            DesignTimeExceptionHandler.ReportException( exception );
        }
    }
#pragma warning restore VSTHRD100

    public async Task ResumeAsync( AsyncExecutionContext executionContext, CancellationToken cancellationToken = default )
    {
        this.Logger.Trace?.Log( $"Resuming the pipeline for project '{this.ProjectKey}'." );

        using ( await this.WithLockAsync( executionContext, cancellationToken ) )
        {
            try
            {
                if ( this.Status != DesignTimeAspectPipelineStatus.Paused )
                {
                    this.Logger.Trace?.Log( $"A Resume request was requested for project '{this.ProjectKey}', but the pipeline was not paused." );

                    return;
                }

                await this.ResumeCoreAsync( executionContext );
            }
            finally
            {
                await this.ProcessJobQueueAsync( executionContext );
            }
        }
    }

    private async ValueTask ResumeCoreAsync( AsyncExecutionContext executionContext )
    {
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
        await this.SetStateAsync( this._currentState.Reset(), executionContext );

        // Notify that the pipeline must be executed again.
        this._jobQueue.Enqueue(
            _ =>
            {
                this._eventHub.OnProjectDirty( this.ProjectKey );

                return default;
            } );
    }

    internal async ValueTask<FallibleResultWithDiagnostics<AspectPipelineConfiguration>> GetConfigurationAsync(
        PartialCompilation compilation,
        bool ignoreStatus,
        AsyncExecutionContext executionContext,
        TestableCancellationToken cancellationToken )
    {
        using ( await this.WithLockAsync( executionContext, cancellationToken ) )
        {
            if ( ignoreStatus )
            {
                await this.InvalidateCacheAsync( compilation.Compilation, executionContext, cancellationToken );
            }

            var state = this._currentState;

            var getConfigurationResult = PipelineState.GetConfiguration(
                ref state,
                compilation,
                ignoreStatus,
                cancellationToken );

            await this.SetStateAsync( state, executionContext );

            await this.ProcessJobQueueAsync( executionContext );

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

    private async ValueTask<ProjectVersion> InvalidateCacheAsync(
        Compilation compilation,
        AsyncExecutionContext executionContext,
        TestableCancellationToken cancellationToken )
    {
        var newState = await this._currentState.InvalidateCacheForNewCompilationAsync(
            compilation,
            true,
            cancellationToken );

        await this.SetStateAsync( newState, executionContext );

        return newState.ProjectVersion.AssertNotNull();
    }

    public async ValueTask ResetCacheAsync( AsyncExecutionContext executionContext, CancellationToken cancellationToken )
    {
        using ( await this.WithLockAsync( executionContext, cancellationToken ) )
        {
            await this.SetStateAsync( this._currentState.Reset(), executionContext );

            this._eventHub.OnProjectDirty( this.ProjectKey );

            await this.ProcessJobQueueAsync( executionContext );
        }
    }

    private async Task<FallibleResultWithDiagnostics<CompilationResult>> ExecutePartialAsync(
        PartialCompilation partialCompilation,
        DesignTimeProjectVersion projectVersion,
        AsyncExecutionContext executionContext,
        TestableCancellationToken cancellationToken )
    {
        var result = await PipelineState.ExecuteAsync( this._currentState, partialCompilation, projectVersion, cancellationToken );

        // Intentionally updating the state atomically after execution of the method, so the state is
        // not affected by a cancellation.
        await this.SetStateAsync( result.NewState, executionContext );

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
        => this._taskRunner.RunSynchronously( () => this.ExecuteAsync( compilation, AsyncExecutionContext.Get(), cancellationToken ), cancellationToken );

    // This method is for testing only.
    public bool TryExecute(
        Compilation compilation,
        TestableCancellationToken cancellationToken,
        [NotNullWhen( true )] out CompilationResult? compilationResult )
    {
        var result = this._taskRunner.RunSynchronously(
            () => this.ExecuteAsync( compilation, AsyncExecutionContext.Get(), cancellationToken ),
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

    internal ValueTask<FallibleResultWithDiagnostics<DesignTimeProjectVersion>> GetDesignTimeProjectVersionAsync(
        Compilation compilation,
        AsyncExecutionContext executionContext,
        TestableCancellationToken cancellationToken )
        => this.GetDesignTimeProjectVersionAsync( compilation, false, executionContext, cancellationToken );

    internal async ValueTask<FallibleResultWithDiagnostics<DesignTimeProjectVersion>> GetDesignTimeProjectVersionAsync(
        Compilation compilation,
        bool autoResumePipeline,
        AsyncExecutionContext executionContext,
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
            if ( this._pipelineFactory.TryGetMetalamaVersion( reference.Compilation, out var metalamaVersion ) )
            {
                if ( metalamaVersion == EngineAssemblyMetadataReader.Instance.AssemblyVersion )
                {
                    // This is a Metalama reference of the current version. We need to compile the dependency.
                    var referenceResult = await this._pipelineFactory.ExecuteAsync(
                        reference.Compilation,
                        autoResumePipeline,
                        executionContext,
                        cancellationToken );

                    if ( !referenceResult.IsSuccessful )
                    {
                        this.Logger.Warning?.Log(
                            $"GetDesignTimeProjectVersionAsync('{this.ProjectKey}'): the pipeline for the reference '{reference.ProjectKey}' failed." );

                        return FallibleResultWithDiagnostics<DesignTimeProjectVersion>.Failed(
                            referenceResult.Diagnostics,
                            $"The pipeline for the reference '{reference.ProjectKey}' failed: {referenceResult.DebugReason}" );
                    }

                    compilationReferences.Add(
                        new DesignTimeProjectReference(
                            referenceResult.Value.ProjectVersion.ProjectKey,
                            referenceResult.Value.TransformationResult ) );

                    if ( referenceResult.Value.PipelineStatus == DesignTimeAspectPipelineStatus.Paused )
                    {
                        pipelineStatus = DesignTimeAspectPipelineStatus.Paused;
                    }
                }
                else
                {
                    // We have a reference to a different version of Metalama.

                    var entryPointConsumer = this._entryPointConsumer.AssertNotNull();
                    var serviceProvider = (await entryPointConsumer.GetServiceProviderAsync( metalamaVersion, cancellationToken )).AssertNotNull();

                    var transitiveCompilationService =
                        (ITransitiveCompilationService) serviceProvider.GetService( typeof(ITransitiveCompilationService) ).AssertNotNull();

                    var resultArray = new ITransitiveCompilationResult?[1];
                    await transitiveCompilationService.GetTransitiveAspectManifestAsync( reference.Compilation, resultArray, cancellationToken );

                    var result = resultArray[0].AssertNotNull();

                    if ( result.IsSuccessful != true )
                    {
                        this.Logger.Warning?.Log( $"Failed to process the reference to '{reference.ProjectKey}': cannot get the transitive aspect manifest." );

                        return FallibleResultWithDiagnostics<DesignTimeProjectVersion>.Failed(
                            result.Diagnostics.ToImmutableArray(),
                            $"GetTransitiveAspectManifest failed for '{reference.ProjectKey}'" );
                    }
                    else
                    {
                        // To deserialize the manifest, we need a service provider with the CompileTimeProject of the referenced project, compiled
                        // for the current Metalama version.

                        var workspaceProvider = this.ServiceProvider.Global.GetRequiredService<WorkspaceProvider>();
                        var referencedProject = await workspaceProvider.GetProjectAsync( reference.ProjectKey, cancellationToken );

                        if ( referencedProject == null )
                        {
                            this.Logger.Warning?.Log(
                                $"Failed to process the reference to '{reference.ProjectKey}': cannot get the project from the workspace." );

                            return FallibleResultWithDiagnostics<DesignTimeProjectVersion>.Failed(
                                ImmutableArray<Diagnostic>.Empty,
                                $"Cannot get the project '{reference.ProjectKey}' from the workspace" );
                        }

                        var pipeline = this._pipelineFactory.GetOrCreatePipeline( referencedProject, cancellationToken );

                        if ( pipeline == null )
                        {
                            this.Logger.Warning?.Log( $"Failed to process the reference to '{reference.ProjectKey}': cannot get a pipeline." );

                            return FallibleResultWithDiagnostics<DesignTimeProjectVersion>.Failed(
                                ImmutableArray<Diagnostic>.Empty,
                                $"Cannot get the pipeline for project '{reference.ProjectKey}'." );
                        }

                        var configuration = await pipeline.GetConfigurationAsync(
                            PartialCompilation.CreateComplete( reference.Compilation ),
                            false,
                            executionContext,
                            cancellationToken );

                        if ( !configuration.IsSuccessful )
                        {
                            return FallibleResultWithDiagnostics<DesignTimeProjectVersion>.Failed(
                                configuration.Diagnostics,
                                $"Cannot get configuration: {configuration.DebugReason}" );
                        }

                        var manifest = TransitiveAspectsManifest.Deserialize( new MemoryStream( result.Manifest! ), configuration.Value.ServiceProvider );

                        compilationReferences.Add(
                            new DesignTimeProjectReference(
                                reference.ProjectKey,
                                manifest ) );

                        if ( result.IsPipelinePaused )
                        {
                            pipelineStatus = DesignTimeAspectPipelineStatus.Paused;
                        }
                    }
                }
            }
            else
            {
                // It is a non-Metalama reference.
                var projectKey = reference.Compilation.GetProjectKey();

                var projectReference = new DesignTimeProjectReference( projectKey );
                compilationReferences.Add( projectReference );
            }
        }

        return new DesignTimeProjectVersion( compilationVersion, compilationReferences, pipelineStatus );
    }

    public ValueTask<FallibleResultWithDiagnostics<CompilationResult>> ExecuteAsync(
        Compilation compilation,
        AsyncExecutionContext executionContext,
        TestableCancellationToken cancellationToken = default )
        => this.ExecuteAsync( compilation, false, executionContext, cancellationToken );

    public async ValueTask<FallibleResultWithDiagnostics<CompilationResult>> ExecuteAsync(
        Compilation compilation,
        bool autoResumePipeline,
        AsyncExecutionContext executionContext,
        TestableCancellationToken cancellationToken = default )
    {
        async Task AutoResumeAsync()
        {
            if ( this.Status == DesignTimeAspectPipelineStatus.Paused && autoResumePipeline )
            {
                await this.ResumeCoreAsync( executionContext );
            }
        }

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
            using ( await this.WithLockAsync( executionContext, cancellationToken ) )
            {
                try
                {
                    if ( this._compilationResultCache.TryGetValue( compilation, out compilationResult ) )
                    {
                        return compilationResult;
                    }

                    var projectVersion = await this.GetDesignTimeProjectVersionAsync( compilation, autoResumePipeline, executionContext, cancellationToken );

                    if ( !projectVersion.IsSuccessful )
                    {
                        // A dependency could not be compiled.
                        this.Logger.Warning?.Log( $"ExecuteAsync('{this.ProjectKey}'): cannot compile a referenced project." );

                        return FallibleResultWithDiagnostics<CompilationResult>.Failed(
                            projectVersion.Diagnostics,
                            $"Cannot compile a referenced project: {projectVersion.DebugReason}" );
                    }

                    // If a dependency project was paused, we must pause too.
                    if ( this.Status != DesignTimeAspectPipelineStatus.Paused
                         && projectVersion.Value.PipelineStatus == DesignTimeAspectPipelineStatus.Paused )
                    {
                        await this.SetStateAsync( this._currentState.Pause(), executionContext );
                    }

                    await AutoResumeAsync();

                    Compilation? compilationToAnalyze = null;

                    if ( this.Status != DesignTimeAspectPipelineStatus.Paused )
                    {
                        // Invalidate the cache for the new compilation.
                        var compilationVersion = await this.InvalidateCacheAsync(
                            compilation,
                            executionContext,
                            cancellationToken );

                        await AutoResumeAsync();

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

                            var executionResult = await this.ExecutePartialAsync(
                                partialCompilation,
                                projectVersion.Value,
                                executionContext,
                                cancellationToken );

                            if ( !executionResult.IsSuccessful )
                            {
                                compilationResult = FallibleResultWithDiagnostics<CompilationResult>.Failed( executionResult.Diagnostics );

                                if ( !this._compilationResultCache.TryAdd( compilation, compilationResult ) )
                                {
                                    // TODO: there seems to be some race which I cannot solve, but it is better not to fail in this case.
                                    this.Logger.Warning?.Log( $"Results of compilation '{this.ProjectKey}' were already in the cache." );
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
                            this._currentState.Status,
                            this._currentState.Configuration!.Value.Value );

                        if ( !this._compilationResultCache.TryAdd( compilation, compilationResult ) )
                        {
                            // TODO: there seems to be some race which I cannot solve, but it is better not to fail in this case.
                            this.Logger.Warning?.Log( $"Results of compilation '{this.ProjectKey}' were already in the cache." );
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
                                this._currentState.Status,
                                this._currentState.PipelineResult.Configuration.AssertNotNull() );
                        }
                        else
                        {
                            // The pipeline was paused before being first executed.
                            compilationResult = FallibleResultWithDiagnostics<CompilationResult>.Failed(
                                ImmutableArray<Diagnostic>.Empty,
                                "The pipeline was paused in the middle of execution." );
                        }

                        return compilationResult;
                    }
                }
                finally
                {
                    await this.ProcessJobQueueAsync( executionContext );
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

    internal IReadOnlyList<AspectClass> GetEligibleAspects( Compilation compilation, ISymbol symbol, TestableCancellationToken cancellationToken )
    {
        var classes = this.AspectClasses;

        if ( classes == null )
        {
            return Array.Empty<AspectClass>();
        }

        // We are not implementing this method as an enumerator for the ease of debugging.
        var result = new List<AspectClass>();

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
                    result.Add( aspectClass );
                }
            }
        }

        return result;
    }

    private async ValueTask ExecuteIfLockAvailableOrEnqueueAsync( Func<AsyncExecutionContext, ValueTask> action, AsyncExecutionContext executionContext )
    {
        using ( var @lock = await this.WithLockAsync( executionContext, 0, CancellationToken.None ) )
        {
            if ( @lock.IsAcquired )
            {
                await action( executionContext );

                await this.ProcessJobQueueAsync( executionContext );
            }
            else
            {
                this._jobQueue.Enqueue( action );

                // Enqueue a task to process the action when the lock will be available.
                _ = this.ProcessJobQueueWhenLockAvailableAsync();
            }
        }
    }

    internal async Task ProcessJobQueueWhenLockAvailableAsync()
    {
        var executionContext = AsyncExecutionContext.Get();

        using ( await this.WithLockAsync( executionContext, CancellationToken.None ) )
        {
            await this.ProcessJobQueueAsync( executionContext );
        }
    }

    private async ValueTask ProcessJobQueueAsync( AsyncExecutionContext executionContext )
    {
        while ( this._mustProcessQueue )
        {
            this._mustProcessQueue = false;

            while ( this._jobQueue.TryDequeue( out var job ) )
            {
                await job( executionContext );
            }
        }
    }

    private ValueTask<Lock> WithLockAsync( AsyncExecutionContext executionContext, CancellationToken cancellationToken )
        => this.WithLockAsync( executionContext, -1, cancellationToken );

    private async ValueTask<Lock> WithLockAsync( AsyncExecutionContext executionContext, int timeout, CancellationToken cancellationToken )
    {
        if ( this._sync.CurrentCount < 1 )
        {
            this.Logger.Trace?.Log( $"Waiting for lock on '{this.ProjectKey}'." );
        }

#if DEBUG
        executionContext.DetectCycle( this );
#endif

        // First wait for a limited amount of time, so we can display a warning in case of large delay.
        var realTimeout = timeout < 0 ? int.MaxValue : timeout;
        var acquired = await this._sync.WaitAsync( Math.Min( 5000, realTimeout ), cancellationToken );

        if ( !acquired )
        {
            this.Logger.Warning?.Log( $"Acquiring the lock on '{this.ProjectKey}' is taking a long time." + Environment.NewLine + new StackTrace() );

            if ( realTimeout > 5000 )
            {
                acquired = await this._sync.WaitAsync( timeout, cancellationToken );
            }
        }

        // Now wait more if needed.
        Action? lockDisposeAction = null;

        if ( acquired )
        {
            this._mustProcessQueue = true;
            this.Logger.Trace?.Log( $"Lock on '{this.ProjectKey}' acquired." );

#if DEBUG

            executionContext.Push( this );

            if ( this.Logger.Warning != null )
            {
                var callStack = new StackTrace();
                var isDisposed = false;
                lockDisposeAction = () => isDisposed = true;

                _ = Task.Delay( TimeSpan.FromSeconds( 10 ), cancellationToken )
                    .ContinueWith(
                        _ =>
                        {
                            if ( !isDisposed )
                            {
                                this.Logger.Warning?.Log( $"The following call stack has been holding the lock for '{this.ProjectKey}' for a long time:" );
                                this.Logger.Warning?.Log( callStack.ToString() );
                            }
                        },
                        cancellationToken,
                        TaskContinuationOptions.None,
                        TaskScheduler.Current );
            }
#endif
        }
        else
        {
            this.Logger.Trace?.Log( $"Lock on '{this.ProjectKey}' not acquired because of a timeout." );
        }

        return new Lock( this, acquired, lockDisposeAction, executionContext );
    }

    private readonly struct Lock : IDisposable
    {
        private readonly DesignTimeAspectPipeline _parent;
        private readonly Action? _disposeAction;
        private readonly AsyncExecutionContext _executionContext;

        public Lock( DesignTimeAspectPipeline sync, bool isAcquired, Action? disposeAction, AsyncExecutionContext executionContext )
        {
            this._parent = sync;
            this.IsAcquired = isAcquired;
            this._disposeAction = disposeAction;
            this._executionContext = executionContext;
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

#if DEBUG
                this._executionContext.Pop( this._parent );
#endif
            }

            this._disposeAction?.Invoke();
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

        var getConfigurationResult = await this.GetConfigurationAsync( partialCompilation, true, AsyncExecutionContext.Get(), cancellationToken );

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