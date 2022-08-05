// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using K4os.Hash.xxHash;
using Metalama.Backstage.Diagnostics;
using Metalama.Framework.DesignTime.Pipeline.Diff;
using Metalama.Framework.Engine;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Options;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Engine.Utilities.Caching;
using Metalama.Framework.Engine.Utilities.Diagnostics;
using Metalama.Framework.Engine.Utilities.Threading;
using Microsoft.CodeAnalysis;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

// ReSharper disable InconsistentlySynchronizedField

namespace Metalama.Framework.DesignTime.Pipeline
{
    /// <summary>
    /// Caches the <see cref="DesignTimeAspectPipeline"/> (so they can be reused between projects) and the
    /// returns produced by <see cref="DesignTimeAspectPipeline"/>. This class is also responsible for invoking
    /// cache invalidation methods as appropriate.
    /// </summary>
    internal class DesignTimeAspectPipelineFactory : IDisposable, IAspectPipelineConfigurationProvider,
                                                     ICompileTimeCodeEditingStatusService
    {
        private readonly ConcurrentDictionary<ProjectKey, DesignTimeAspectPipeline> _pipelinesByProjectKey = new();
        private readonly ILogger _logger;
        private readonly ConcurrentQueue<TaskCompletionSource<DesignTimeAspectPipeline>> _newPipelineListeners = new();
        private readonly CancellationToken _globalCancellationToken = CancellationToken.None;
        private readonly ConditionalWeakTable<SyntaxTree, StrongBox<ulong>> _syntaxTreeHashes = new();
        private readonly ServiceProvider _serviceProvider;

        private readonly bool _isTest;

        private volatile int _numberOfPipelinesEditingCompileTimeCode;

        public CompileTimeDomain Domain { get; }

        public DesignTimeAspectPipelineFactory( ServiceProvider serviceProvider, CompileTimeDomain domain, bool isTest = false )
        {
            this.Domain = domain;
            this._serviceProvider = serviceProvider;
            this._isTest = isTest;
            this._logger = serviceProvider.GetLoggerFactory().GetLogger( "DesignTime" );
        }

        /// <summary>
        /// Gets the pipeline for a given project, and creates it if necessary.
        /// </summary>
        internal DesignTimeAspectPipeline? GetOrCreatePipeline( IProjectOptions projectOptions, Compilation compilation, CancellationToken cancellationToken )
        {
            if ( !projectOptions.IsFrameworkEnabled )
            {
                this._logger.Trace?.Log( $"Cannot get a pipeline for project '{projectOptions.AssemblyName}': Metalama is disabled for this project." );

                return null;
            }

            // We lock the dictionary because the ConcurrentDictionary does not guarantee that the creation delegate
            // is called only once, and we prefer a single instance for the simplicity of debugging. 

            var compilationId = ProjectKey.FromCompilation( compilation );

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

                    var serviceProvider = this._serviceProvider.WithServices( projectOptions, this );
                    pipeline = new DesignTimeAspectPipeline( serviceProvider, this.Domain, compilationId, compilation.References, this._isTest );
                    pipeline.PipelineResumed += this.OnPipelineResumed;
                    pipeline.StatusChanged += this.OnPipelineStatusChanged;

                    if ( !this._pipelinesByProjectKey.TryAdd( compilationId, pipeline ) )
                    {
                        throw new AssertionFailedException();
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

        Task ICompileTimeCodeEditingStatusService.OnEditingCompileTimeCodeCompletedAsync()
        {
            Logger.DesignTime.Trace?.Log( "Received ICompileTimeCodeEditingStatusService.OnEditingCompileTimeCodeCompleted." );

            var tasks = new List<Task>( this._pipelinesByProjectKey.Values.Count );

            foreach ( var pipeline in this._pipelinesByProjectKey.Values )
            {
                tasks.Add( pipeline.ResumeAsync( true, this._globalCancellationToken ).AsTask() );
            }

            return Task.WhenAll( tasks );
        }

        public bool IsUserInterfaceAttached { get; private set; }

        void ICompileTimeCodeEditingStatusService.OnUserInterfaceAttached()
        {
            this.IsUserInterfaceAttached = true;
        }

        private void OnPipelineStatusChanged( DesignTimePipelineStatusChangedEventArgs args )
        {
            var wasEditing = args.OldStatus == DesignTimeAspectPipelineStatus.Paused;
            var isEditing = args.NewStatus == DesignTimeAspectPipelineStatus.Paused;

            if ( wasEditing && !isEditing )
            {
                if ( Interlocked.Decrement( ref this._numberOfPipelinesEditingCompileTimeCode ) == 0 )
                {
                    this.IsEditingCompileTimeCode = false;
                    this.IsEditingCompileTimeCodeChanged?.Invoke( false );
                }
            }
            else if ( !wasEditing && isEditing )
            {
                if ( Interlocked.Increment( ref this._numberOfPipelinesEditingCompileTimeCode ) == 1 )
                {
                    this.IsEditingCompileTimeCode = true;
                    this.IsEditingCompileTimeCodeChanged?.Invoke( true );
                }
            }
        }

        private void OnPipelineResumed( object sender, EventArgs e )
        {
            // If a build has started, we have to invalidate the whole cache because we have allowed
            // our cache to become inconsistent when we started to have an outdated pipeline configuration.
            foreach ( var pipeline in this._pipelinesByProjectKey.Values )
            {
                _ = pipeline.InvalidateCacheAsync( this._globalCancellationToken );
            }
        }

        public bool TryExecute(
            IProjectOptions options,
            Compilation compilation,
            CancellationToken cancellationToken,
            [NotNullWhen( true )] out CompilationResult? compilationResult )
        {
            compilationResult = TaskHelper.RunAndWait( () => this.ExecuteAsync( options, compilation, cancellationToken ), cancellationToken );

            return compilationResult != null;
        }

        public Task<CompilationResult?> ExecuteAsync(
            IProjectOptions projectOptions,
            Compilation compilation,
            CancellationToken cancellationToken )
        {
            // Force to create the pipeline.
            var designTimePipeline = this.GetOrCreatePipeline( projectOptions, compilation, cancellationToken );

            if ( designTimePipeline == null )
            {
                return Task.FromResult<CompilationResult?>( null );
            }

            // Call the execution method that assumes that the pipeline exists or waits for it.
            return this.ExecuteAsync( compilation, cancellationToken );
        }

        protected virtual bool HasMetalamaReference( Compilation compilation ) => ProjectKey.FromCompilation( compilation ).HasMetalama;

        private async Task<CompilationResult?> ExecuteAsync( Compilation compilation, CancellationToken cancellationToken )
        {
            if ( !this.HasMetalamaReference( compilation ) )
            {
                return null;
            }

            List<DesignTimeCompilationReference> compilationReferences = new();

            foreach ( var reference in compilation.References.OfType<CompilationReference>() )
            {
                if ( this.HasMetalamaReference( reference.Compilation ) )
                {
                    // This is a Metalama reference. We need to compile the dependency.
                    var referenceResult = await this.ExecuteAsync( reference.Compilation, cancellationToken );

                    if ( referenceResult == null )
                    {
                        return null;
                    }

                    compilationReferences.Add(
                        new DesignTimeCompilationReference(
                            referenceResult.CompilationVersion,
                            referenceResult.TransformationResult ) );
                }
                else
                {
                    // It is a non-Metalama reference. 
                    compilationReferences.Add(
                        new DesignTimeCompilationReference( new NonMetalamaCompilationVersion( reference.Compilation, this.ComputeSyntaxTreeHash ) ) );
                }
            }

            var referenceCollection = new DesignTimeCompilationReferenceCollection( compilationReferences );

            var pipeline = await this.GetPipelineAndWaitAsync( compilation, cancellationToken );

            if ( pipeline == null )
            {
                return null;
            }

            return await pipeline.ExecuteAsync( compilation, referenceCollection, cancellationToken );
        }

        private ulong ComputeSyntaxTreeHash( SyntaxTree syntaxTree ) => this._syntaxTreeHashes.GetOrAdd( syntaxTree, this.ComputeSyntaxTreeHashCore ).Value;

        private StrongBox<ulong> ComputeSyntaxTreeHashCore( SyntaxTree syntaxTree )
        {
            XXH64 hash = new();
            var hasher = new RunTimeCodeHasher( hash );
            hasher.Visit( syntaxTree.GetRoot() );

            return new StrongBox<ulong>( hash.Digest() );
        }

        public void Dispose()
        {
            foreach ( var designTimeAspectPipeline in this._pipelinesByProjectKey.Values )
            {
                designTimeAspectPipeline.Dispose();
            }

            this._pipelinesByProjectKey.Clear();
            this.Domain.Dispose();
        }

        protected virtual async ValueTask<DesignTimeAspectPipeline?> GetPipelineAndWaitAsync( Compilation compilation, CancellationToken cancellationToken )
        {
            var projectKey = ProjectKey.FromCompilation( compilation );

            DesignTimeAspectPipeline? pipeline;

            while ( !this._pipelinesByProjectKey.TryGetValue( projectKey, out pipeline ) )
            {
                this._logger.Trace?.Log( $"Awaiting for the pipeline '{projectKey}'." );

                var taskCompletionSource = new TaskCompletionSource<DesignTimeAspectPipeline>();

                using ( cancellationToken.Register( () => taskCompletionSource.SetCanceled() ) )
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

        async ValueTask<AspectPipelineConfiguration?> IAspectPipelineConfigurationProvider.GetConfigurationAsync(
            PartialCompilation compilation,
            IDiagnosticAdder diagnosticAdder,
            CancellationToken cancellationToken )
        {
            var pipeline = await this.GetPipelineAndWaitAsync( compilation.Compilation, cancellationToken );

            if ( pipeline == null )
            {
                return null;
            }

            return await pipeline.GetConfigurationAsync( compilation, diagnosticAdder, true, cancellationToken );
        }
    }
}