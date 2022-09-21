// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.AspectOrdering;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Engine.Validation;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;

namespace Metalama.Framework.Engine.CodeFixes
{
    /// <summary>
    /// The implementation of <see cref="HighLevelPipelineStage"/> for <see cref="CodeFixPipeline"/>.
    /// </summary>
    internal class CodeFixPipelineStage : HighLevelPipelineStage
    {
        public CodeFixPipelineStage( CompileTimeProject compileTimeProject, IReadOnlyList<OrderedAspectLayer> aspectLayers, IServiceProvider serviceProvider ) :
            base( compileTimeProject, aspectLayers, serviceProvider ) { }

        protected override AspectPipelineResult GetStageResult(
            AspectPipelineConfiguration pipelineConfiguration,
            AspectPipelineResult input,
            IPipelineStepsResult pipelineStepsResult,
            CancellationToken cancellationToken )
            => new(
                input.Compilation,
                input.Project,
                input.AspectLayers,
                input.CompilationModels.AddRange( pipelineStepsResult.Compilations ),
                
                // TODO: Is this twice intentionally?
                input.Diagnostics.Concat( pipelineStepsResult.Diagnostics ).Concat( pipelineStepsResult.Diagnostics ),
                input.AspectSources.AddRange( pipelineStepsResult.ExternalAspectSources ),
                default,
                pipelineStepsResult.InheritableAspectInstances,
                ImmutableArray<ReferenceValidatorInstance>.Empty,
                input.AdditionalSyntaxTrees,
                input.AspectInstanceResults );
    }
}