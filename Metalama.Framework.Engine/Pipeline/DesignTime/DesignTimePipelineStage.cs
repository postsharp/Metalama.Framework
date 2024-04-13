// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.AspectOrdering;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.HierarchicalOptions;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Utilities.Threading;
using Metalama.Framework.Engine.Validation;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace Metalama.Framework.Engine.Pipeline.DesignTime
{
    /// <summary>
    /// An implementation of <see cref="DesignTimePipelineStage"/> called from source generators.
    /// </summary>
    internal sealed class DesignTimePipelineStage : HighLevelPipelineStage
    {
        private readonly ProjectServiceProvider _serviceProvider;

        public DesignTimePipelineStage(
            CompileTimeProject compileTimeProject,
            IReadOnlyList<OrderedAspectLayer> aspectLayers,
            ProjectServiceProvider serviceProvider )
            : base( compileTimeProject, aspectLayers )
        {
            this._serviceProvider = serviceProvider;
        }

        /// <inheritdoc/>
        protected override async Task<AspectPipelineResult> GetStageResultAsync(
            AspectPipelineConfiguration pipelineConfiguration,
            AspectPipelineResult input,
            IPipelineStepsResult pipelineStepsResult,
            TestableCancellationToken cancellationToken )
        {
            var diagnosticSink = new UserDiagnosticSink( this.CompileTimeProject, null );

            // Discover the validators.
            bool hasDeclarationValidator;
            ImmutableArray<ReferenceValidatorInstance> referenceValidators;

            var validatorSources = pipelineStepsResult.ValidatorSources;

            if ( !validatorSources.IsEmpty )
            {
                var validatorRunner = new ValidationRunner( pipelineConfiguration, validatorSources );
                var initialCompilation = pipelineStepsResult.FirstCompilation;
                var finalCompilation = pipelineStepsResult.LastCompilation;

                hasDeclarationValidator = await validatorRunner.RunDeclarationValidatorsAsync(
                    initialCompilation,
                    finalCompilation,
                    diagnosticSink,
                    cancellationToken );

                referenceValidators = (await validatorRunner.GetReferenceValidatorsAsync( initialCompilation, diagnosticSink, cancellationToken ))
                    .ToImmutableArray();
            }
            else
            {
                hasDeclarationValidator = false;
                referenceValidators = ImmutableArray<ReferenceValidatorInstance>.Empty;
            }

            // Generate the additional syntax trees.

            var additionalSyntaxTrees = await DesignTimeSyntaxTreeGenerator.GenerateDesignTimeSyntaxTreesAsync(
                this._serviceProvider,
                input.LastCompilation,
                pipelineStepsResult.FirstCompilation,
                pipelineStepsResult.LastCompilation,
                pipelineStepsResult.Transformations,
                diagnosticSink,
                cancellationToken );

            return
                new AspectPipelineResult(
                    input.LastCompilation,
                    input.Project,
                    input.AspectLayers,
                    input.FirstCompilationModel.AssertNotNull(),
                    pipelineStepsResult.LastCompilation,
                    input.Configuration,
                    input.Diagnostics.Concat( pipelineStepsResult.Diagnostics ).Concat( diagnosticSink.ToImmutable() ),
                    new PipelineContributorSources(
                        input.ContributorSources.AspectSources.AddRange( pipelineStepsResult.OverflowAspectSources ),
                        validatorSources,
                        ImmutableArray<IHierarchicalOptionsSource>.Empty ),
                    pipelineStepsResult.InheritableAspectInstances,
                    pipelineStepsResult.LastCompilation.Annotations,
                    hasDeclarationValidator,
                    referenceValidators,
                    input.AdditionalSyntaxTrees.AddRange( additionalSyntaxTrees ),
                    input.AspectInstanceResults.AddRange( pipelineStepsResult.AspectInstanceResults ),
                    transformations: pipelineStepsResult.Transformations.ToImmutableArray<ITransformationBase>() );
        }
    }
}