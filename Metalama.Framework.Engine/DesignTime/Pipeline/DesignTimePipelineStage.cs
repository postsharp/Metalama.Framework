// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.AspectOrdering;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Engine.Validation;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;

namespace Metalama.Framework.Engine.DesignTime.Pipeline
{
    /// <summary>
    /// An implementation of <see cref="DesignTimePipelineStage"/> called from source generators.
    /// </summary>
    internal class DesignTimePipelineStage : HighLevelPipelineStage
    {
        public DesignTimePipelineStage(
            CompileTimeProject compileTimeProject,
            IReadOnlyList<OrderedAspectLayer> aspectLayers,
            IServiceProvider serviceProvider )
            : base( compileTimeProject, aspectLayers, serviceProvider ) { }

        /// <inheritdoc/>
        protected override PipelineStageResult GetStageResult(
            AspectPipelineConfiguration pipelineConfiguration,
            PipelineStageResult input,
            IPipelineStepsResult pipelineStepsResult,
            CancellationToken cancellationToken )
        {
            var diagnosticSink = new UserDiagnosticSink( this.CompileTimeProject, null );

            // Discover the validators.
            ImmutableArray<ReferenceValidatorInstance> referenceValidators;

            if ( !pipelineStepsResult.ValidatorSources.IsDefaultOrEmpty )
            {
                var validatorRunner = new ValidationRunner( pipelineConfiguration, pipelineStepsResult.ValidatorSources, cancellationToken );
                var initialCompilation = pipelineStepsResult.Compilations[0];
                var finalCompilation = pipelineStepsResult.Compilations[pipelineStepsResult.Compilations.Length - 1];
                validatorRunner.RunDeclarationValidators( finalCompilation, diagnosticSink );
                referenceValidators = validatorRunner.GetReferenceValidators( initialCompilation, diagnosticSink ).ToImmutableArray();
            }
            else
            {
                referenceValidators = ImmutableArray<ReferenceValidatorInstance>.Empty;
            }

            // Generate the additional syntax trees.

            DesignTimeSyntaxTreeGenerator.GenerateDesignTimeSyntaxTrees(
                input.Compilation,
                pipelineStepsResult.LastCompilation,
                this.ServiceProvider,
                diagnosticSink,
                cancellationToken,
                out var additionalSyntaxTrees );

            return new PipelineStageResult(
                input.Compilation,
                input.Project,
                input.AspectLayers,
                input.CompilationModels.AddRange( pipelineStepsResult.Compilations ),
                input.Diagnostics.Concat( pipelineStepsResult.Diagnostics ).Concat( diagnosticSink.ToImmutable() ),
                input.AspectSources.AddRange( pipelineStepsResult.ExternalAspectSources ),
                input.ValidatorSources.AddRange( pipelineStepsResult.ValidatorSources ),
                pipelineStepsResult.InheritableAspectInstances,
                referenceValidators,
                input.AdditionalSyntaxTrees.AddRange( additionalSyntaxTrees ) );
        }
    }
}