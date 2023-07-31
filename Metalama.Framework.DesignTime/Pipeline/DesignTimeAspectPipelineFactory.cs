// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Configuration;
using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Extensibility;
using Metalama.Framework.DesignTime.Pipeline.Diff;
using Metalama.Framework.DesignTime.Rpc;
using Metalama.Framework.DesignTime.Utilities;
using Metalama.Framework.Engine;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Configuration;
using Metalama.Framework.Engine.Options;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Engine.Pipeline.DesignTime;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities.Diagnostics;
using Metalama.Framework.Engine.Utilities.Threading;
using Metalama.Framework.Project;
using Metalama.Framework.Services;
using Microsoft.CodeAnalysis;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

// ReSharper disable InconsistentlySynchronizedField
// ReSharper disable ClassWithVirtualMembersNeverInherited.Global

namespace Metalama.Framework.DesignTime.Pipeline;

/// <summary>
/// Caches the <see cref="DesignTimeAspectPipeline"/> (so they can be reused between projects) and the
/// returns produced by <see cref="DesignTimeAspectPipeline"/>. This class is also responsible for invoking
/// cache invalidation methods as appropriate.
/// </summary>
internal class DesignTimeAspectPipelineFactory : IDisposable, IAspectPipelineConfigurationProvider
{
    private readonly ConcurrentDictionary<ProjectKey, DesignTimeAspectPipeline> _pipelinesByProjectKey = new();
    private readonly ILogger _logger;
    private readonly ConcurrentQueue<TaskCompletionSource<DesignTimeAspectPipeline>> _newPipelineListeners = new();
    private readonly CancellationToken _globalCancellationToken = CancellationToken.None;
    private readonly IMetalamaProjectClassifier _projectClassifier;
    private readonly AnalysisProcessEventHub _eventHub;
    private readonly IProjectOptionsFactory _projectOptionsFactory;
    private readonly ITaskRunner _taskRunner;

    public ServiceProvider<IGlobalService> ServiceProvider { get; }

    public CompileTimeDomain Domain { get; }

    public DesignTimeAspectPipelineFactory( ServiceProvider<IGlobalService> serviceProvider, CompileTimeDomain domain )
    {
        this._projectClassifier = serviceProvider.GetRequiredService<IMetalamaProjectClassifier>();
        serviceProvider = serviceProvider.WithService( this );

        this._projectOptionsFactory = serviceProvider.GetRequiredService<IProjectOptionsFactory>();

        this._taskRunner = serviceProvider.GetRequiredService<ITaskRunner>();

        this._eventHub = serviceProvider.GetRequiredService<AnalysisProcessEventHub>();
        this._eventHub.EditingCompileTimeCodeCompleted += this.OnEditingCompileTimeCodeCompleted;
        this._eventHub.ExternalBuildCompletedEvent.RegisterHandler( this.OnExternalBuildCompletedAsync );
        serviceProvider = serviceProvider.WithServices( new ProjectVersionProvider( serviceProvider ) );

        this.Domain = domain;
        this.ServiceProvider = serviceProvider;
        this._logger = serviceProvider.GetLoggerFactory().GetLogger( "DesignTime" );

        // Write the design-time configuration file if it doesn't exist, so metalama-config can open it.
        serviceProvider.GetRequiredBackstageService<IConfigurationManager>().CreateIfMissing<DesignTimeConfiguration>();
    }

#pragma warning disable VSTHRD100
    private async void OnEditingCompileTimeCodeCompleted()
    {
        try
        {
            await this.ResumePipelinesAsync( AsyncExecutionContext.Get(), true, CancellationToken.None );
        }
        catch ( Exception e )
        {
            DesignTimeExceptionHandler.ReportException( e, this._logger );
        }
    }
#pragma warning restore VSTHRD100

    /// <summary>
    /// Gets the pipeline for a given project, and creates it if necessary.
    /// </summary>
    public DesignTimeAspectPipeline? GetOrCreatePipeline(
        Microsoft.CodeAnalysis.Project project,
        TestableCancellationToken cancellationToken = default )
    {
        var projectKey = ProjectKeyFactory.FromProject( project );

        if ( projectKey == null )
        {
            return null;
        }

        if ( this.TryGetPipeline( projectKey, out var pipeline ) )
        {
            return pipeline;
        }
        else
        {
            var options = this._projectOptionsFactory.GetProjectOptions( project );

            return this.GetOrCreatePipeline( options, projectKey, project.MetadataReferences.OfType<PortableExecutableReference>(), cancellationToken );
        }
    }

    /// <summary>
    /// Gets the pipeline for a given compilation, and creates it if necessary.
    /// </summary>
    internal DesignTimeAspectPipeline? GetOrCreatePipeline(
        IProjectOptions projectOptions,
        Compilation compilation,
        TestableCancellationToken cancellationToken = default )
        => this.GetOrCreatePipeline(
            projectOptions,
            ProjectKeyFactory.FromCompilation( compilation ),
            compilation.References.OfType<PortableExecutableReference>(),
            cancellationToken );

    private DesignTimeAspectPipeline? GetOrCreatePipeline(
        IProjectOptions projectOptions,
        ProjectKey projectKey,
        IEnumerable<PortableExecutableReference> references,
        TestableCancellationToken cancellationToken = default )
    {
        if ( !projectOptions.IsFrameworkEnabled )
        {
            this._logger.Trace?.Log( $"Cannot get a pipeline for project '{projectOptions.AssemblyName}': Metalama is disabled for this project." );

            return null;
        }

        // We lock the dictionary because the ConcurrentDictionary does not guarantee that the creation delegate
        // is called only once, and we prefer a single instance for the simplicity of debugging. 

        if ( this._pipelinesByProjectKey.TryGetValue( projectKey, out var pipeline ) )
        {
            // TODO: we must validate that the project options and metadata references are still identical to those cached, otherwise we should create a new pipeline.
            return pipeline;
        }
        else
        {
            lock ( this._pipelinesByProjectKey )
            {
                if ( this._pipelinesByProjectKey.TryGetValue( projectKey, out pipeline ) )
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    return pipeline;
                }

                pipeline = new DesignTimeAspectPipeline( this, projectOptions, projectKey, references );

                if ( !this._pipelinesByProjectKey.TryAdd( projectKey, pipeline ) )
                {
                    throw new AssertionFailedException( $"The pipeline '{projectKey}' has already been created." );
                }

                foreach ( var listener in this._newPipelineListeners )
                {
                    listener.TrySetResult( pipeline );
                }

                return pipeline;
            }
        }
    }

    public async ValueTask ResumePipelinesAsync( AsyncExecutionContext executionContext, bool executePipelineNow, CancellationToken cancellationToken )
    {
        Logger.DesignTime.Trace?.Log( "Received ICompileTimeCodeEditingStatusService.OnEditingCompileTimeCodeCompleted." );

        // Resuming all pipelines.
        var tasks = new List<Task>();

        foreach ( var pipeline in this._pipelinesByProjectKey.Values )
        {
            tasks.Add( pipeline.ResumeAsync( executionContext.Fork(), executePipelineNow, cancellationToken ) );
        }

        await Task.WhenAll( tasks );
    }

    private async Task OnExternalBuildCompletedAsync( ProjectKey projectKey )
    {
        // If a build has started, we have to invalidate the whole cache because we have allowed
        // our cache to become inconsistent when we started to have an outdated pipeline configuration.
        foreach ( var pipeline in this._pipelinesByProjectKey.Values )
        {
            // We don't do it concurrently because ResetCacheAsync is most likely synchronous.

            await pipeline.ResetCacheAsync( AsyncExecutionContext.Get(), this._globalCancellationToken );
        }

        // In case the event hub got out of sync (which should not happen), we reset its status.
        this._eventHub.ResetIsEditingCompileTimeCode();
    }

    public bool TryExecute(
        IProjectOptions options,
        Compilation compilation,
        TestableCancellationToken cancellationToken,
        [NotNullWhen( true )] out AspectPipelineResultAndState? compilationResult )
        => this.TryExecute( options, compilation, cancellationToken, out compilationResult, out _ );

    public bool TryExecute(
        IProjectOptions options,
        Compilation compilation,
        TestableCancellationToken cancellationToken,
        [NotNullWhen( true )] out AspectPipelineResultAndState? compilationResult,
        out ImmutableArray<Diagnostic> diagnostics )
    {
        var result = this._taskRunner.RunSynchronously(
            () => this.ExecuteAsync( options, compilation, AsyncExecutionContext.Get(), cancellationToken ),
            cancellationToken );

        if ( result.IsSuccessful )
        {
            compilationResult = result.Value;
            diagnostics = result.Diagnostics;

            return true;
        }
        else
        {
            compilationResult = null;
            diagnostics = result.Diagnostics;

            return false;
        }
    }

    private Task<FallibleResultWithDiagnostics<AspectPipelineResultAndState>> ExecuteAsync(
        IProjectOptions projectOptions,
        Compilation compilation,
        AsyncExecutionContext executionContext,
        TestableCancellationToken cancellationToken )
    {
        // Force to create the pipeline.
        var designTimePipeline = this.GetOrCreatePipeline( projectOptions, compilation, cancellationToken );

        if ( designTimePipeline == null )
        {
            return Task.FromResult( FallibleResultWithDiagnostics<AspectPipelineResultAndState>.Failed( ImmutableArray<Diagnostic>.Empty ) );
        }

        // Call the execution method that assumes that the pipeline exists or waits for it.
        return this.ExecuteAsync( compilation, executionContext, cancellationToken );
    }

    public virtual bool TryGetMetalamaVersion( Compilation compilation, [NotNullWhen( true )] out Version? version )
        => this._projectClassifier.TryGetMetalamaVersion( compilation, out version );

    internal Task<FallibleResultWithDiagnostics<AspectPipelineResultAndState>> ExecuteAsync(
        Compilation compilation,
        AsyncExecutionContext executionContext,
        TestableCancellationToken cancellationToken = default )
        => this.ExecuteAsync( compilation, false, executionContext, cancellationToken );

    internal async Task<FallibleResultWithDiagnostics<AspectPipelineResultAndState>> ExecuteAsync(
        Compilation compilation,
        bool autoResumePipeline,
        AsyncExecutionContext executionContext,
        TestableCancellationToken cancellationToken = default )
    {
        var pipeline = await this.GetPipelineAndWaitAsync( compilation, cancellationToken );

        if ( pipeline == null )
        {
            return FallibleResultWithDiagnostics<AspectPipelineResultAndState>.Failed( "Cannot get the pipeline." );
        }

        return await pipeline.ExecuteAsync( compilation, autoResumePipeline, executionContext, cancellationToken );
    }

    public virtual void Dispose()
    {
        foreach ( var designTimeAspectPipeline in this._pipelinesByProjectKey.Values )
        {
            designTimeAspectPipeline.Dispose();
        }

        this._eventHub.ExternalBuildCompletedEvent.UnregisterHandler( this.OnExternalBuildCompletedAsync );
        this._pipelinesByProjectKey.Clear();
        this.Domain.Dispose();
    }

    public virtual async ValueTask<DesignTimeAspectPipeline?> GetPipelineAndWaitAsync( Compilation compilation, CancellationToken cancellationToken )
    {
        var projectKey = compilation.GetProjectKey();

        DesignTimeAspectPipeline? pipeline;

        while ( !this._pipelinesByProjectKey.TryGetValue( projectKey, out pipeline ) )
        {
            if ( !this.TryGetMetalamaVersion( compilation, out _ ) )
            {
                this._logger.Trace?.Log( "Metalama is not enabled for this project." );

                return null;
            }

            this._logger.Trace?.Log( $"Awaiting for the pipeline '{projectKey}'." );

            var taskCompletionSource = new TaskCompletionSource<DesignTimeAspectPipeline>();

#if NET6_0_OR_GREATER
            await using ( cancellationToken.Register( () => taskCompletionSource.SetCanceled( cancellationToken ) ) )
#else
            using ( cancellationToken.Register( () => taskCompletionSource.SetCanceled() ) )
#endif
            {
                this._newPipelineListeners.Enqueue( taskCompletionSource );
            }

            await taskCompletionSource.Task;
        }

        return pipeline;
    }

    public bool TryGetPipeline( ProjectKey projectKey, [NotNullWhen( true )] out DesignTimeAspectPipeline? pipeline )
    {
        if ( !this._pipelinesByProjectKey.TryGetValue( projectKey, out pipeline ) )
        {
            this._logger.Trace?.Log( $"Cannot get the pipeline for project '{projectKey}': it has not been created yet." );

            return false;
        }

        return true;
    }

    async ValueTask<FallibleResultWithDiagnostics<AspectPipelineConfiguration>> IAspectPipelineConfigurationProvider.GetConfigurationAsync(
        PartialCompilation compilation,
        AsyncExecutionContext executionContext,
        TestableCancellationToken cancellationToken )
    {
        var pipeline = await this.GetPipelineAndWaitAsync( compilation.Compilation, cancellationToken );

        if ( pipeline == null )
        {
            return FallibleResultWithDiagnostics<AspectPipelineConfiguration>.Failed( ImmutableArray<Diagnostic>.Empty );
        }

        var configuration = await pipeline.GetConfigurationAsync( compilation, true, executionContext, cancellationToken );

        if ( configuration.IsSuccessful )
        {
            var transitiveAspectManifestProvider = await pipeline.GetDesignTimeProjectVersionAsync(
                compilation.Compilation,
                autoResumePipeline: false,
                executionContext,
                cancellationToken );

            if ( !transitiveAspectManifestProvider.IsSuccessful )
            {
                return FallibleResultWithDiagnostics<AspectPipelineConfiguration>.Failed(
                    transitiveAspectManifestProvider.Diagnostics,
                    transitiveAspectManifestProvider.DebugReason );
            }

            configuration = configuration.Value.WithServiceProvider(
                configuration.Value.ServiceProvider.WithService( transitiveAspectManifestProvider.Value ) );
        }

        return configuration;
    }
}