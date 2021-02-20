using Caravela.Framework.Impl.AspectOrdering;
using System.Collections.Generic;
using System.Linq;
using Caravela.Framework.Impl.CodeModel.Builders;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Linking;

namespace Caravela.Framework.Impl.Pipeline
{
    /// <summary>
    /// The implementation of <see cref="HighLevelAspectsPipelineStage"/> used at compile time (not at design time).
    /// </summary>
    internal class CompileTimeHighLevelAspectsPipelineStage : HighLevelAspectsPipelineStage
    {
        public CompileTimeHighLevelAspectsPipelineStage( IReadOnlyList<OrderedAspectLayer> aspectLayers, CompileTimeAssemblyLoader assemblyLoader, IAspectPipelineProperties properties ) 
            : base( aspectLayers, assemblyLoader, properties )
        {
        }

        /// <inheritdoc/>
        protected override PipelineStageResult GenerateCode( PipelineStageResult input, IPipelineStepsResult pipelineStepResult )
        {
            var linker = new AspectLinker( new AdviceLinkerInput( input.Compilation, pipelineStepResult.Compilation, pipelineStepResult.NonObservableTransformations, input.AspectLayers ) );
            var linkerResult = linker.ToResult();

            return new PipelineStageResult(
                linkerResult.Compilation,
                input.AspectLayers,
                pipelineStepResult.Diagnostics.Concat( linkerResult.Diagnostics ).ToList(),
                pipelineStepResult.NonObservableTransformations.OfType<ManagedResourceBuilder>().Select( r => r.ToResourceDescription() ).ToList(),
                pipelineStepResult.ExternalAspectSources );
        }
    }
}