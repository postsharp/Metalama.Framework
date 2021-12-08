// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Impl.AspectOrdering;
using Metalama.Framework.Impl.Collections;
using Metalama.Framework.Impl.CompileTime;
using Metalama.Framework.Impl.Diagnostics;
using Metalama.Framework.Impl.Pipeline;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Metalama.Framework.Impl.DesignTime.Pipeline
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
                input.AspectSources.Concat( pipelineStepsResult.ExternalAspectSources ),
                pipelineStepsResult.InheritableAspectInstances,
                input.AdditionalSyntaxTrees.Concat( additionalSyntaxTrees ) );
        }
    }
}