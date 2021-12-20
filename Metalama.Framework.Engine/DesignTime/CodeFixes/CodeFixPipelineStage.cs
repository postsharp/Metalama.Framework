// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.AspectOrdering;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Engine.Validation;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;

namespace Metalama.Framework.Engine.DesignTime.CodeFixes
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
                input.CompilationModels.AddRange( pipelineStepsResult.Compilations ),
                input.Diagnostics.Concat( pipelineStepsResult.Diagnostics ).Concat( pipelineStepsResult.Diagnostics ),
                input.AspectSources.AddRange( pipelineStepsResult.ExternalAspectSources ),
                default,
                pipelineStepsResult.InheritableAspectInstances,
                ImmutableArray<ReferenceValidatorInstance>.Empty,
                input.AdditionalSyntaxTrees );
    }
}