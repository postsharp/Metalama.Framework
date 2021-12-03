// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.AspectOrdering;
using Caravela.Framework.Impl.Collections;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Pipeline;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Caravela.Framework.Impl.DesignTime.CodeFixes
{
    /// <summary>
    /// The implementation of <see cref="HighLevelPipelineStage"/> for <see cref="CodeFixPipeline"/>.
    /// </summary>
    internal class CodeFixPipelineStage : HighLevelPipelineStage
    {
        public CodeFixPipelineStage( CompileTimeProject compileTimeProject, IReadOnlyList<OrderedAspectLayer> aspectLayers, IServiceProvider serviceProvider ) :
            base( compileTimeProject, aspectLayers, serviceProvider )
        { }

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