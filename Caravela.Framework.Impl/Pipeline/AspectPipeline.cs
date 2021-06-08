// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.AspectOrdering;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Options;
using Caravela.Framework.Sdk;
using Microsoft.CodeAnalysis;
using MoreLinq;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;

namespace Caravela.Framework.Impl.Pipeline
{
    /// <summary>
    /// The base class for the main process of Caravela.
    /// </summary>
    public abstract class AspectPipeline : IDisposable, IAspectPipelineProperties
    {
        public IProjectOptions ProjectOptions { get; }

        private readonly CompileTimeDomain _domain;

        public ServiceProvider ServiceProvider { get; }

        IServiceProvider IAspectPipelineProperties.ServiceProvider => this.ServiceProvider;

        protected AspectPipeline(
            IProjectOptions projectOptions,
            CompileTimeDomain domain,
            IDirectoryOptions? directoryOptions = null,
            IAssemblyLocator? assemblyLocator = null )
        {
            this._domain = domain;
            this.ProjectOptions = projectOptions;
            this.ServiceProvider = ServiceProviderFactory.GetServiceProvider( directoryOptions, assemblyLocator );
        }

        internal int PipelineInitializationCount { get; set; }

        private protected bool TryInitialize(
            IDiagnosticAdder diagnosticAdder,
            PartialCompilation compilation,
            IReadOnlyList<SyntaxTree>? compileTimeTreesHint,
            CancellationToken cancellationToken,
            [NotNullWhen( true )] out AspectPipelineConfiguration? configuration )
        {
            this.PipelineInitializationCount++;

            var roslynCompilation = compilation.Compilation;

            // Create dependencies.
            var loader = CompileTimeProjectLoader.Create( this._domain, this.ServiceProvider );

            // Prepare the compile-time assembly.
            if ( !loader.TryGetCompileTimeProject(
                roslynCompilation,
                compileTimeTreesHint,
                diagnosticAdder,
                false,
                cancellationToken,
                out var compileTimeProject ) )
            {
                configuration = null;

                return false;
            }

            // Create aspect types.
            var driverFactory = new AspectDriverFactory( compilation.Compilation, this.ProjectOptions.PlugIns );
            var aspectTypeFactory = new AspectClassMetadataFactory( driverFactory );

            var aspectTypes = aspectTypeFactory.GetAspectClasses( compilation.Compilation, compileTimeProject, diagnosticAdder ).ToImmutableArray();

            // Get aspect parts and sort them.
            var unsortedAspectLayers = aspectTypes
                .Where( t => !t.IsAbstract )
                .SelectMany( at => at.Layers )
                .ToImmutableArray();

            var aspectOrderSources = new IAspectOrderingSource[]
            {
                new AttributeAspectOrderingSource( compilation.Compilation, loader ),
                new AspectLayerOrderingSource( aspectTypes )
            };

            if ( !AspectLayerSorter.TrySort( unsortedAspectLayers, aspectOrderSources, diagnosticAdder, out var sortedAspectLayers ) )
            {
                configuration = null;

                return false;
            }

            var aspectLayers = sortedAspectLayers.ToImmutableArray();

            var stages = aspectLayers
                .GroupAdjacent( x => GetGroupingKey( x.AspectClass.AspectDriver ) )
                .Select( g => this.CreateStage( g.Key, g.ToImmutableArray(), loader, compileTimeProject! ) )
                .ToImmutableArray();

            configuration = new AspectPipelineConfiguration( stages, aspectTypes, sortedAspectLayers, compileTimeProject, loader );

            return true;

            static object GetGroupingKey( IAspectDriver driver )
                => driver switch
                {
                    // weavers are not grouped together
                    // Note: this requires that every AspectType has its own instance of IAspectWeaver
                    IAspectWeaver weaver => weaver,

                    // AspectDrivers are grouped together
                    AspectDriver => nameof(AspectDriver),

                    _ => throw new NotSupportedException()
                };
        }

        private protected virtual IReadOnlyList<IAspectSource> CreateAspectSources( AspectPipelineConfiguration configuration )
        {
            return new[] { new CompilationAspectSource( configuration.AspectClasses, configuration.CompileTimeProjectLoader ) };
        }

        /// <summary>
        /// Executes the all stages of the current pipeline, report diagnostics, and returns the last <see cref="PipelineStageResult"/>.
        /// </summary>
        /// <returns><c>true</c> if there was no error, <c>false</c> otherwise.</returns>
        private protected bool TryExecute(
            PartialCompilation compilation,
            IDiagnosticAdder diagnosticAdder,
            AspectPipelineConfiguration pipelineConfiguration,
            CancellationToken cancellationToken,
            [NotNullWhen( true )] out PipelineStageResult? pipelineStageResult )
        {
            // If there is no aspect in the compilation, don't execute the pipeline.
            if ( pipelineConfiguration.CompileTimeProject == null || pipelineConfiguration.AspectClasses.Count == 0 )
            {
                pipelineStageResult = new PipelineStageResult( compilation, Array.Empty<OrderedAspectLayer>() );

                return true;
            }

            var aspectSources = this.CreateAspectSources( pipelineConfiguration );

            pipelineStageResult = new PipelineStageResult( compilation, pipelineConfiguration.Layers, aspectSources: aspectSources );

            foreach ( var stage in pipelineConfiguration.Stages )
            {
                if ( !stage.TryExecute( pipelineStageResult!, diagnosticAdder, cancellationToken, out var newStageResult ) )
                {
                    return false;
                }
                else
                {
                    pipelineStageResult = newStageResult;
                }
            }

            var hasError = pipelineStageResult.Diagnostics.ReportedDiagnostics.Any( d => d.Severity >= DiagnosticSeverity.Error );

            foreach ( var diagnostic in pipelineStageResult.Diagnostics.ReportedDiagnostics )
            {
                cancellationToken.ThrowIfCancellationRequested();
                diagnosticAdder.Report( diagnostic );
            }

            return !hasError;
        }

        /// <summary>
        /// Creates an instance of <see cref="HighLevelPipelineStage"/>.
        /// </summary>
        /// <param name="parts"></param>
        /// <param name="compileTimeProject"></param>
        /// <param name="compileTimeProjectLoader"></param>
        /// <returns></returns>
        private protected abstract HighLevelPipelineStage CreateStage(
            IReadOnlyList<OrderedAspectLayer> parts,
            CompileTimeProject compileTimeProject,
            CompileTimeProjectLoader compileTimeProjectLoader );

        private PipelineStage CreateStage(
            object groupKey,
            IReadOnlyList<OrderedAspectLayer> parts,
            CompileTimeProjectLoader compileTimeProjectLoader,
            CompileTimeProject compileTimeProject )
        {
            switch ( groupKey )
            {
                case IAspectWeaver weaver:

                    var partData = parts.Single();

                    return new LowLevelPipelineStage( weaver, partData.AspectClass, this );

                case nameof(AspectDriver):

                    return this.CreateStage( parts, compileTimeProject, compileTimeProjectLoader );

                default:

                    throw new NotSupportedException();
            }
        }

        protected virtual void Dispose( bool disposing ) { }

        /// <inheritdoc/>
        public void Dispose() => this.Dispose( true );

        public abstract bool CanTransformCompilation { get; }
    }
}