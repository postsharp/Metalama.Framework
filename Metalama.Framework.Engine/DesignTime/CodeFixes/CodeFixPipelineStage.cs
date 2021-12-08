// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Impl.AspectOrdering;
using Metalama.Framework.Impl.Collections;
using Metalama.Framework.Impl.CompileTime;
using Metalama.Framework.Impl.Pipeline;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Metalama.Framework.Impl.DesignTime.CodeFixes
{
    /// <summary>
    /// The implementation of <see cref="HighLevelPipelineStage"/> for <see cref="CodeFixPipeline"/>.
    /// </summary>
    internal class CodeFixPipelineStage : HighLevelPipelineStage
    {
        public CodeFixPipelineStage( CompileTimeProject compileTimeProject, IReadOnlyList<OrderedAspectLayer> aspectLayers, IServiceProvider serviceProvider ) :
            base( compileTimeProject, aspectLayers, serviceProvider ) { }

        protected override PipelineStageResult GetStageResult(
            AspectPipelineConfiguration pipelineConfiguration,
            PipelineStageResult input,
            IPipelineStepsResult pipelineStepsResult,
            CancellationToken cancellationToken )
            => new(
                input.Compilation,
                input.Project,
                input.AspectLayers,
                pipelineStepsResult.Compilation,
                input.Diagnostics.Concat( pipelineStepsResult.Diagnostics ).Concat( pipelineStepsResult.Diagnostics ),
                input.AspectSources.Concat( pipelineStepsResult.ExternalAspectSources ),
                pipelineStepsResult.InheritableAspectInstances,
                input.AdditionalSyntaxTrees );
    }
}