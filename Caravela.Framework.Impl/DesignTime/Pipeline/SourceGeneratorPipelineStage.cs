// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.AspectOrdering;
using Caravela.Framework.Impl.Collections;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Pipeline;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Caravela.Framework.Impl.DesignTime.Pipeline
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
                cancellationToken,
                diagnosticSink,
                out var additionalSyntaxTrees );

            return new PipelineStageResult(
                input.Compilation,
                input.Project,
                input.AspectLayers,
                input.CompilationModel,
                input.Diagnostics.Concat( pipelineStepsResult.Diagnostics ).Concat( diagnosticSink.ToImmutable() ),
                input.AspectSources.Concat( pipelineStepsResult.ExternalAspectSources ),
                pipelineStepsResult.InheritableAspectInstances,
                input.AdditionalSyntaxTrees.Concat( additionalSyntaxTrees ) );
        }
    }
}