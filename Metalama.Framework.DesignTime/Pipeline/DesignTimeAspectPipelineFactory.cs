// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Framework.DesignTime.Licensing;
using Metalama.Framework.Engine;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Options;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Engine.Utilities.Diagnostics;
using Microsoft.CodeAnalysis;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

// ReSharper disable InconsistentlySynchronizedField

namespace Metalama.Framework.DesignTime.Pipeline
{
    /// <summary>
    /// Caches the <see cref="DesignTimeAspectPipeline"/> (so they can be reused between projects) and the
    /// returns produced by <see cref="DesignTimeAspectPipeline"/>. This class is also responsible for invoking
    /// cache invalidation methods as appropriate.
    /// </summary>
    internal class DesignTimeAspectPipelineFactory : IDisposable, ITransitiveAspectManifestProvider, IAspectPipelineConfigurationProvider,
                                                     ICompileTimeCodeEditingStatusService
    {
        private readonly ConcurrentDictionary<string, DesignTimeAspectPipeline> _pipelinesByProjectId = new();
        private readonly ILogger _logger;

        public IServiceProvider ServiceProvider { get; }

        private readonly bool _isTest;

        private volatile int _numberOfPipelinesEditingCompileTimeCode;

        public CompileTimeDomain Domain { get; }

        public DesignTimeAspectPipelineFactory( IServiceProvider serviceProvider, CompileTimeDomain domain, bool isTest = false )
        {
            this.Domain = domain;
            this.ServiceProvider = serviceProvider;
            this._isTest = isTest;
            this._logger = serviceProvider.GetLoggerFactory().GetLogger( "DesignTime" );
        }

        protected virtual string GetProjectId( IProjectOptions projectOptions, Compilation compilation ) => projectOptions.ProjectId;

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

            var projectId = this.GetProjectId( projectOptions, compilation );

            if ( this._pipelinesByProjectId.TryGetValue( projectId, out var pipeline ) )
            {
                // TODO: we must validate that the project options and metadata references are still identical to those cached, otherwise we should create a new pipeline.
                return pipeline;
            }
            else
            {
                lock ( this._pipelinesByProjectId )
                {
                    if ( this._pipelinesByProjectId.TryGetValue( projectId, out pipeline ) )
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        return pipeline;
                    }

                    var serviceProvider = ServiceProviderFactory.GetServiceProvider()
                        .WithServices( projectOptions, this );

                    serviceProvider = serviceProvider
                        .WithService( new DesignTimeLicenseVerifier( serviceProvider ) );
                    
                    pipeline = new DesignTimeAspectPipeline( serviceProvider, this.Domain, compilation.References, this._isTest );
                    pipeline.PipelineResumed += this.OnPipelineResumed;
                    pipeline.StatusChanged += this.OnPipelineStatusChanged;

                    if ( !this._pipelinesByProjectId.TryAdd( projectId, pipeline ) )
                    {
                        throw new AssertionFailedException();
                    }

                    return pipeline;
                }
            }
        }

        private bool IsEditingCompileTimeCode { get; set; }

        bool ICompileTimeCodeEditingStatusService.IsEditingCompileTimeCode => this.IsEditingCompileTimeCode;

        public event Action<bool>? IsEditingCompileTimeCodeChanged;

        void ICompileTimeCodeEditingStatusService.OnEditingCompileTimeCodeCompleted()
        {
            Logger.DesignTime.Trace?.Log( "Received ICompileTimeCodeEditingStatusService.OnEditingCompileTimeCodeCompleted." );

            foreach ( var pipeline in this._pipelinesByProjectId.Values )
            {
                pipeline.Resume( true );
            }
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
            foreach ( var pipeline in this._pipelinesByProjectId.Values )
            {
                pipeline.InvalidateCache();
            }
        }

        public IEnumerable<AspectClass> GetEligibleAspects(
            Compilation compilation,
            ISymbol symbol,
            IProjectOptions projectOptions,
            CancellationToken cancellationToken )
        {
            var pipeline = this.GetOrCreatePipeline( projectOptions, compilation, cancellationToken );

            if ( pipeline == null )
            {
                return Enumerable.Empty<AspectClass>();
            }

            return pipeline.GetEligibleAspects( compilation, symbol, cancellationToken );
        }

        public bool TryExecute(
            IProjectOptions projectOptions,
            Compilation compilation,
            CancellationToken cancellationToken,
            [NotNullWhen( true )] out CompilationResult? compilationResult )
        {
            var designTimePipeline = this.GetOrCreatePipeline( projectOptions, compilation, cancellationToken );

            if ( designTimePipeline == null )
            {
                compilationResult = null;

                return false;
            }

            return designTimePipeline.TryExecute( compilation, cancellationToken, out compilationResult );
        }

        public void Dispose()
        {
            foreach ( var designTimeAspectPipeline in this._pipelinesByProjectId.Values )
            {
                designTimeAspectPipeline.Dispose();
            }

            this._pipelinesByProjectId.Clear();
            this.Domain.Dispose();
        }

        public virtual bool TryGetPipeline( Compilation compilation, [NotNullWhen( true )] out DesignTimeAspectPipeline? pipeline )
        {
            if ( !ProjectIdHelper.TryGetProjectId( compilation, out var projectId ) )
            {
                // The compilation does not reference our package.
                pipeline = null;

                return false;
            }

            if ( !this._pipelinesByProjectId.TryGetValue( projectId, out pipeline ) )
            {
                this._logger.Trace?.Log( $"Cannot get the pipeline for project '{projectId}': it has not been created yet." );

                return false;
            }

            return true;
        }

        public bool TryGetPipeline( string projectId, [NotNullWhen( true )] out DesignTimeAspectPipeline? pipeline )
        {
            if ( !this._pipelinesByProjectId.TryGetValue( projectId, out pipeline ) )
            {
                this._logger.Trace?.Log( $"Cannot get the pipeline for project '{projectId}': it has not been created yet." );

                return false;
            }

            return true;
        }

        public ITransitiveAspectsManifest? GetTransitiveAspectsManifest( Compilation compilation, CancellationToken cancellationToken )
        {
            if ( !this.TryGetPipeline( compilation, out var pipeline ) )
            {
                // We cannot create the pipeline because we don't have all options.
                // If this is a problem, we will need to pass all options as AssemblyMetadataAttribute.

                return null;
            }

            if ( !pipeline.TryExecute( compilation, cancellationToken, out var compilationResult ) )
            {
                return null;
            }

            return compilationResult.PipelineResult;
        }

        bool IAspectPipelineConfigurationProvider.TryGetConfiguration(
            PartialCompilation compilation,
            IDiagnosticAdder diagnosticAdder,
            CancellationToken cancellationToken,
            [NotNullWhen( true )] out AspectPipelineConfiguration? configuration )
        {
            if ( !this.TryGetPipeline( compilation.Compilation, out var pipeline ) )
            {
                configuration = null;

                return false;
            }

            return pipeline.TryGetConfiguration( compilation, diagnosticAdder, true, cancellationToken, out configuration );
        }
    }
}