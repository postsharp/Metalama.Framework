// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.AspectOrdering;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Utilities.Threading;
using Metalama.Framework.Engine.Validation;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace Metalama.Framework.Engine.Pipeline
{
    /// <summary>
    /// An implementation of <see cref="HighLevelPipelineStage"/> that does not do anything but append results.
    /// </summary>
    internal sealed class NullPipelineStage : HighLevelPipelineStage
    {
        public NullPipelineStage(
            CompileTimeProject compileTimeProject,
            IReadOnlyList<OrderedAspectLayer> aspectLayers ) :
            base( compileTimeProject, aspectLayers ) { }

        protected override Task<AspectPipelineResult> GetStageResultAsync(
            AspectPipelineConfiguration pipelineConfiguration,
            AspectPipelineResult input,
            IPipelineStepsResult pipelineStepsResult,
            TestableCancellationToken cancellationToken )
            => Task.FromResult(
                new AspectPipelineResult(
                    input.Compilation,
                    input.Project,
                    input.AspectLayers,
                    input.FirstCompilationModel ?? pipelineStepsResult.FirstCompilation,
                    pipelineStepsResult.LastCompilation,
                    input.Diagnostics.Concat( pipelineStepsResult.Diagnostics ),
                    new PipelineContributorSources(
                        input.ContributorSources.AspectSources.AddRange( pipelineStepsResult.ExternalAspectSources ),
                        input.ContributorSources.ValidatorSources.AddRange( pipelineStepsResult.ValidatorSources ),
                        input.ContributorSources.OptionsSources ),
                    pipelineStepsResult.InheritableAspectInstances,
                    ImmutableArray<ReferenceValidatorInstance>.Empty,
                    input.AdditionalSyntaxTrees,
                    input.AspectInstanceResults ) );
    }
}