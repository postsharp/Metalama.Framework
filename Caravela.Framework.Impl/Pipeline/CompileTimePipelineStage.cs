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
        protected override PipelineStageResult GetStageResult(
            AspectPipelineConfiguration pipelineConfiguration,
            PipelineStageResult input,
            IPipelineStepsResult pipelineStepsResult,
            CancellationToken cancellationToken )
        {
            var linker = new AspectLinker(
                pipelineConfiguration.ServiceProvider,
                new AspectLinkerInput(
                    input.Compilation,
                    pipelineStepsResult.Compilation,
                    pipelineStepsResult.NonObservableTransformations,
                    input.AspectLayers,
                    input.Diagnostics.DiagnosticSuppressions.Concat( pipelineStepsResult.Diagnostics.DiagnosticSuppressions ),
                    this._compileTimeProject ) );

            var linkerResult = linker.ToResult();

            return new PipelineStageResult(
                linkerResult.Compilation,
                input.Project,
                input.AspectLayers,
                null,
                pipelineStepsResult.Diagnostics.Concat( linkerResult.Diagnostics ),
                pipelineStepsResult.ExternalAspectSources,
                input.ExternallyInheritableAspects.AddRange( pipelineStepsResult.InheritableAspectInstances ) );
        }
    }
}