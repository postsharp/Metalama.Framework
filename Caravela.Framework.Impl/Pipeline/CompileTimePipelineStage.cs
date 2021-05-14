// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.AspectOrdering;
using Caravela.Framework.Impl.CodeModel.Builders;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Linking;
using System.Collections.Generic;
using System.Linq;

namespace Caravela.Framework.Impl.Pipeline
{
    /// <summary>
    /// The implementation of <see cref="HighLevelPipelineStage"/> used at compile time (not at design time).
    /// </summary>
    internal class CompileTimePipelineStage : HighLevelPipelineStage
    {
        private readonly CompileTimeProject _compileTimeProject;

        public CompileTimePipelineStage(
            CompileTimeProject compileTimeProject,
            IReadOnlyList<OrderedAspectLayer> aspectLayers,
            IAspectPipelineProperties properties )
            : base( compileTimeProject, aspectLayers, properties )
        {
            this._compileTimeProject = compileTimeProject;
        }

        /// <inheritdoc/>
        protected override PipelineStageResult GenerateCode( PipelineStageResult input, IPipelineStepsResult pipelineStepResult )
        {
            var linker = new AspectLinker(
                this.PipelineProperties.ServiceProvider,
                new AspectLinkerInput(
                    input.PartialCompilation,
                    pipelineStepResult.Compilation,
                    pipelineStepResult.NonObservableTransformations,
                    input.AspectLayers,
                    input.Diagnostics.DiagnosticSuppressions.Concat( pipelineStepResult.Diagnostics.DiagnosticSuppressions ),
                    this._compileTimeProject ) );

            var linkerResult = linker.ToResult();

            return new PipelineStageResult(
                linkerResult.Compilation,
                input.AspectLayers,
                pipelineStepResult.Diagnostics.Concat( linkerResult.Diagnostics ),
                pipelineStepResult.NonObservableTransformations.OfType<ManagedResourceBuilder>().Select( r => r.ToResourceDescription() ).ToList(),
                pipelineStepResult.ExternalAspectSources );
        }
    }
}