// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.AspectOrdering;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Observers;
using Metalama.Framework.Engine.Sdk;
using Metalama.Framework.Project;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Metalama.Framework.Engine.Pipeline
{
    /// <summary>
    /// A <see cref="PipelineStage"/> that groups all aspects written with the high-level API instead of
    /// the <see cref="IAspectWeaver"/>.
    /// </summary>
    internal abstract class HighLevelPipelineStage : PipelineStage
    {
        protected CompileTimeProject CompileTimeProject { get; }

        private readonly IReadOnlyList<OrderedAspectLayer> _aspectLayers;

        protected HighLevelPipelineStage(
            CompileTimeProject compileTimeProject,
            IReadOnlyList<OrderedAspectLayer> aspectLayers,
            IServiceProvider serviceProvider ) : base( serviceProvider )
        {
            this.CompileTimeProject = compileTimeProject;
            this._aspectLayers = aspectLayers;
        }

        /// <inheritdoc/>
        public override bool TryExecute(
            AspectPipelineConfiguration pipelineConfiguration,
            PipelineStageResult input,
            IDiagnosticAdder diagnostics,
            CancellationToken cancellationToken,
            [NotNullWhen( true )] out PipelineStageResult? result )
        {
            var compilation = CompilationModel.CreateInitialInstance( input.Project, input.Compilation );

            this.ServiceProvider.GetService<ICompilationModelObserver>()?.OnInitialCompilationModelCreated( compilation );

            var pipelineStepsState = new PipelineStepsState(
                this._aspectLayers,
                compilation,
                input.AspectSources,
                pipelineConfiguration );

            pipelineStepsState.Execute( cancellationToken );

            result = this.GetStageResult( pipelineConfiguration, input, pipelineStepsState, cancellationToken );

            return true;
        }

        /// <summary>
        /// Generates the code required by the aspects whose execution resulted in a given <see cref="IPipelineStepsResult"/>, and combine it with an input
        /// <see cref="PipelineStageResult"/> to produce an output <see cref="PipelineStageResult"/>.
        /// </summary>
        /// <param name="pipelineConfiguration"></param>
        /// <param name="input"></param>
        /// <param name="pipelineStepsResult"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected abstract PipelineStageResult GetStageResult(
            AspectPipelineConfiguration pipelineConfiguration,
            PipelineStageResult input,
            IPipelineStepsResult pipelineStepsResult,
            CancellationToken cancellationToken );
    }
}