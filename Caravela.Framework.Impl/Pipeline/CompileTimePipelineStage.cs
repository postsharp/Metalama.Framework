// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.AspectOrdering;
using Caravela.Framework.Impl.Collections;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Linking;
using System;
using System.Collections.Generic;
using System.Threading;

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
            IServiceProvider serviceProvider )
            : base( compileTimeProject, aspectLayers, serviceProvider )
        {
            this._compileTimeProject = compileTimeProject;
        }

        /// <inheritdoc/>
        protected override PipelineStageResult GenerateCode(
            PipelineStageResult input,
            IPipelineStepsResult pipelineStepResult,
            CancellationToken cancellationToken )
        {
            var linker = new AspectLinker(
                this.ServiceProvider,
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
                pipelineStepResult.ExternalAspectSources );
        }
    }
}