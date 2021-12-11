// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.AspectOrdering;
using Metalama.Framework.Engine.Collections;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Pipeline;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Metalama.Framework.Engine.DesignTime.Pipeline
{
    /// <summary>
    /// An implementation of <see cref="SourceGeneratorPipelineStage"/> called from source generators.
    /// </summary>
    internal class SourceGeneratorPipelineStage : HighLevelPipelineStage
    {
        public SourceGeneratorPipelineStage(
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

            DesignTimeSyntaxTreeGenerator.GenerateDesignTimeSyntaxTrees(
                input.Compilation,
                pipelineStepsResult.Compilation,
                this.ServiceProvider,
                diagnosticSink,
                cancellationToken,
                out var additionalSyntaxTrees );

            return new PipelineStageResult(
                input.Compilation,
                input.Project,
                input.AspectLayers,
                input.CompilationModel,
                input.Diagnostics.Concat( pipelineStepsResult.Diagnostics ).Concat( diagnosticSink.ToImmutable() ),
                input.AspectSources.AddRange( pipelineStepsResult.ExternalAspectSources ),
                input.ValidatorSources.AddRange( pipelineStepsResult.ValidatorSources ),
                pipelineStepsResult.InheritableAspectInstances,
                input.AdditionalSyntaxTrees.AddRange( additionalSyntaxTrees ) );
        }
    }
}