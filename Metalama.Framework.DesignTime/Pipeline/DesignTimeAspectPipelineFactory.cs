// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Configuration;
using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Extensibility;
using Metalama.Framework.DesignTime.Pipeline.Diff;
using Metalama.Framework.DesignTime.Utilities;
using Metalama.Framework.Engine;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Configuration;
using Metalama.Framework.Engine.Options;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Engine.Pipeline.DesignTime;
using Metalama.Framework.Engine.Utilities.Diagnostics;
using Metalama.Framework.Engine.Utilities.Threading;
using Microsoft.CodeAnalysis;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

// ReSharper disable InconsistentlySynchronizedField

namespace Metalama.Framework.DesignTime.Pipeline
{
    /// <summary>
    /// Caches the <see cref="DesignTimeAspectPipeline"/> (so they can be reused between projects) and the
    /// returns produced by <see cref="DesignTimeAspectPipeline"/>. This class is also responsible for invoking
    /// cache invalidation methods as appropriate.
    /// </summary>
    internal class DesignTimeAspectPipelineFactory : IDisposable, IAspectPipelineConfigurationProvider,
                                                     ICompileTimeCodeEditingStatusService, IMetalamaProjectClassifier
    {
        private readonly ConcurrentDictionary<ProjectKey, DesignTimeAspectPipeline> _pipelinesByProjectKey = new();
        private readonly ConcurrentDictionary<ProjectKey, NonMetalamaProjectTracker> _nonMetalamaProjectTrackers = new();
        private readonly ILogger _logger;
        private readonly ConcurrentQueue<TaskCompletionSource<DesignTimeAspectPipeline>> _newPipelineListeners = new();
        private readonly CancellationToken _globalCancellationToken = CancellationToken.None;
        private readonly MetalamaProjectClassifier _projectClassifier;

        public ServiceProvider ServiceProvider { get; }

        private readonly bool _isTest;

        private volatile int _numberOfPipelinesEditingCompileTimeCode;

        public CompileTimeDomain Domain { get; }

        public DesignTimeAspectPipelineFactory( ServiceProvider serviceProvider, CompileTimeDomain domain, bool isTest = false )
        {
            this._projectClassifier = new MetalamaProjectClassifier();
            serviceProvider = serviceProvider.WithService( this );
            serviceProvider = serviceProvider.WithService( new ProjectVersionProvider( serviceProvider ) );

            this.Domain = domain;
            this.ServiceProvider = serviceProvider.WithService( this );
            this._isTest = isTest;
            this._logger = serviceProvider.GetLoggerFactory().GetLogger( "DesignTime" );

            // Write the design-time configuration file if it doesn't exist, so metalama-config can open it.
            serviceProvider.GetRequiredBackstageService<IConfigurationManager>().CreateIfMissing<DesignTimeConfiguration>();
        }

        /// <summary>
        /// Gets the pipeline for a given project, and creates it if necessary.
        /// </summary>
        internal DesignTimeAspectPipeline? GetOrCreatePipeline(
            IProjectOptions projectOptions,
            Compilation compilation,
            CancellationToken cancellationToken = default )
        {
            if ( !projectOptions.IsFrameworkEnabled )
            {
                this._logger.Trace?.Log( $"Cannot get a pipeline for project '{projectOptions.AssemblyName}': Metalama is disabled for this project." );

                return null;
            }

            // We lock the dictionary because the ConcurrentDictionary does not guarantee that the creation delegate
            // is called only once, and we prefer a single instance for the simplicity of debugging. 

            var compilationId = compilation.GetProjectKey();

            if ( this._pipelinesByProjectKey.TryGetValue( compilationId, out var pipeline ) )
            {
                // TODO: we must validate that the project options and metadata references are still identical to those cached, otherwise we should create a new pipeline.
                return pipeline;
            }
            else
            {
                lock ( this._pipelinesByProjectKey )
                {
                    if ( this._pipelinesByProjectKey.TryGetValue( compilationId, out pipeline ) )
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        return pipeline;
                    }

                    pipeline = new DesignTimeAspectPipeline( this, projectOptions, compilationId, compilation.References, this._isTest );

                    pipeline.StatusChanged.RegisterHandler( this.OnPipelineStatusChanged );
                    pipeline.ExternalBuildCompletedEvent.RegisterHandler( this.OnExternalBuildCompleted );

                    if ( !this._pipelinesByProjectKey.TryAdd( compilationId, pipeline ) )
                    {
                        throw new AssertionFailedException( $"The pipeline '{compilationId}' has already been created." );
                    }

                    foreach ( var listener in this._newPipelineListeners )
                    {
                        listener.TrySetResult( pipeline );
                    }

                    return pipeline;
                }
            }
        }

        private bool IsEditingCompileTimeCode { get; set; }

        bool ICompileTimeCodeEditingStatusService.IsEditingCompileTimeCode => this.IsEditingCompileTimeCode;

        public event Action<bool>? IsEditingCompileTimeCodeChanged;

        Task ICompileTimeCodeEditingStatusService.OnEditingCompileTimeCodeCompletedAsync( CancellationToken cancellationToken )
            => this.ResumePipelinesAsync( cancellationToken ).AsTask();

        public async ValueTask ResumePipelinesAsync( CancellationToken cancellationToken )
        {
            Logger.DesignTime.Trace?.Log( "Received ICompileTimeCodeEditingStatusService.OnEditingCompileTimeCodeCompleted." );

            // Resuming all pipelines.
            foreach ( var pipeline in this._pipelinesByProjectKey.Values )
            {
                await pipeline.ResumeAsync( cancellationToken );
            }
        }

        public bool IsUserInterfaceAttached { get; private set; }

        void ICompileTimeCodeEditingStatusService.OnUserInterfaceAttached()
        {
            this.IsUserInterfaceAttached = true;
        }

        private async Task OnPipelineStatusChanged( DesignTimePipelineStatusChangedEventArgs args )
        {
            if ( args.IsResuming )
            {
                if ( Interlocked.Decrement( ref this._numberOfPipelinesEditingCompileTimeCode ) == 0 )
                {
                    this.IsEditingCompileTimeCode = false;
                    this.IsEditingCompileTimeCodeChanged?.Invoke( false );
                }
            }
            else if ( args.IsPausing )
            {
                if ( Interlocked.Increment( ref this._numberOfPipelinesEditingCompileTimeCode ) == 1 )
                {
                    this.IsEditingCompileTimeCode = true;
                    this.IsEditingCompileTimeCodeChanged?.Invoke( true );
                }
            }

            await this.PipelineStatusChangedEvent.InvokeAsync( args );
        }

        /// <summary>
        /// Gets an event raised when the pipeline result has changed because of an external cause, i.e.
        /// not a change in the source code of the project of the pipeline itself.
        /// </summary>
        public AsyncEvent<DesignTimePipelineStatusChangedEventArgs> PipelineStatusChangedEvent { get; } = new();

        private async Task OnExternalBuildCompleted( ProjectKey projectKey )
        {
            // If a build has started, we have to invalidate the whole cache because we have allowed
            // our cache to become inconsistent when we started to have an outdated pipeline configuration.
            foreach ( var pipeline in this._pipelinesByProjectKey.Values )
            {
                // We don't do it concurrently because ResetCacheAsync is most likely synchronous.

                await pipeline.ResetCacheAsync( this._globalCancellationToken );
            }
        }

        public bool TryExecute(
            IProjectOptions options,
            Compilation compilation,
            CancellationToken cancellationToken,
            [NotNullWhen( true )] out CompilationResult? compilationResult )
            => this.TryExecute( options, compilation, cancellationToken, out compilationResult, out _ );

        public bool TryExecute(
            IProjectOptions options,
            Compilation compilation,
            CancellationToken cancellationToken,
            [NotNullWhen( true )] out CompilationResult? compilationResult,
            out ImmutableArray<Diagnostic> diagnostics )
        {
            var result = TaskHelper.RunAndWait( () => this.ExecuteAsync( options, compilation, cancellationToken ), cancellationToken );

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

        public Task<FallibleResultWithDiagnostics<CompilationResult>> ExecuteAsync(
            IProjectOptions projectOptions,
            Compilation compilation,
            CancellationToken cancellationToken )
        {
            // Force to create the pipeline.
            var designTimePipeline = this.GetOrCreatePipeline( projectOptions, compilation, cancellationToken );

            if ( designTimePipeline == null )
            {
                return Task.FromResult( FallibleResultWithDiagnostics<CompilationResult>.Failed( ImmutableArray<Diagnostic>.Empty ) );
            }

            // Call the execution method that assumes that the pipeline exists or waits for it.
            return this.ExecuteAsync( compilation, cancellationToken );
        }

        public virtual bool IsMetalamaEnabled( Compilation compilation ) => this._projectClassifier.IsMetalamaEnabled( compilation );

        internal async Task<FallibleResultWithDiagnostics<CompilationResult>> ExecuteAsync(
            Compilation compilation,
            CancellationToken cancellationToken = default )
        {
            var pipeline = await this.GetPipelineAndWaitAsync( compilation, cancellationToken );

            if ( pipeline == null )
            {
                return default;
            }

            return await pipeline.ExecuteAsync( compilation, cancellationToken );
        }

        public virtual void Dispose()
        {
            foreach ( var designTimeAspectPipeline in this._pipelinesByProjectKey.Values )
            {
                designTimeAspectPipeline.Dispose();
            }

            this._pipelinesByProjectKey.Clear();
            this.Domain.Dispose();
        }

        internal NonMetalamaProjectTracker GetNonMetalamaProjectTracker( ProjectKey projectKey )
            => this._nonMetalamaProjectTrackers.GetOrAdd( projectKey, _ => new NonMetalamaProjectTracker( this.ServiceProvider ) );

        protected virtual async ValueTask<DesignTimeAspectPipeline?> GetPipelineAndWaitAsync( Compilation compilation, CancellationToken cancellationToken )
        {
            var projectKey = compilation.GetProjectKey();

            DesignTimeAspectPipeline? pipeline;

            while ( !this._pipelinesByProjectKey.TryGetValue( projectKey, out pipeline ) )
            {
                if ( !this.IsMetalamaEnabled( compilation ) )
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
            CancellationToken cancellationToken )
        {
            var pipeline = await this.GetPipelineAndWaitAsync( compilation.Compilation, cancellationToken );

            if ( pipeline == null )
            {
                return FallibleResultWithDiagnostics<AspectPipelineConfiguration>.Failed( ImmutableArray<Diagnostic>.Empty );
            }

            return await pipeline.GetConfigurationAsync( compilation, true, cancellationToken );
        }
    }
}