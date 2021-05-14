// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.AspectOrdering;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Sdk;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Caravela.Framework.Impl.Pipeline
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
            IAspectPipelineProperties properties ) : base( properties )
        {
            this.CompileTimeProject = compileTimeProject;
            this._aspectLayers = aspectLayers;
        }

        /// <inheritdoc/>
        public override bool TryExecute( PipelineStageResult input, IDiagnosticAdder diagnostics, [NotNullWhen( true )] out PipelineStageResult? result )
        {
            var compilation = CompilationModel.CreateInitialInstance( input.PartialCompilation );

            var pipelineStepsState = new PipelineStepsState(
                this._aspectLayers,
                compilation,
                this.CompileTimeProject,
                input.AspectSources );

            pipelineStepsState.Execute();

            result = this.GenerateCode( input, pipelineStepsState );

            return true;
        }

        /// <summary>
        /// Generates the code required by the aspects whose execution resulted in a given <see cref="IPipelineStepsResult"/>, and combine it with an input
        /// <see cref="PipelineStageResult"/> to produce an output <see cref="PipelineStageResult"/>.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="pipelineStepResult"></param>
        /// <returns></returns>
        protected abstract PipelineStageResult GenerateCode( PipelineStageResult input, IPipelineStepsResult pipelineStepResult );
    }
}