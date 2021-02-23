using System.Collections.Generic;
using Caravela.Framework.Impl.AspectOrdering;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Sdk;

namespace Caravela.Framework.Impl.Pipeline
{
    /// <summary>
    /// A <see cref="PipelineStage"/> that groups all aspects written with the high-level API instead of
    /// the <see cref="IAspectWeaver"/>.
    /// </summary>
    internal abstract class HighLevelPipelineStage : PipelineStage
    {
        private readonly CompileTimeAssemblyLoader _assemblyLoader;
        private readonly IReadOnlyList<OrderedAspectLayer> _aspectLayers;

        protected HighLevelPipelineStage( IReadOnlyList<OrderedAspectLayer> aspectLayers, CompileTimeAssemblyLoader assemblyLoader, IAspectPipelineProperties properties ) : base( properties )
        {
            this._aspectLayers = aspectLayers;
            this._assemblyLoader = assemblyLoader;
        }

        /// <inheritdoc/>
        public override PipelineStageResult Execute( PipelineStageResult input )
        {
            var compilation = CompilationModel.CreateInitialInstance( input.Compilation );
            var pipelineStepsState = new PipelineStepsState(
                this._aspectLayers,
                compilation,
                input.AspectSources.Concat( new CompilationAspectSource( compilation, this._assemblyLoader ) ) );
            pipelineStepsState.Execute();

            return this.GenerateCode( input, pipelineStepsState );
        }

        /// <summary>
        /// Generates the code required by the aspects whose execution resulted in a given <see cref="IPipelineStepsResult"/>, and combine it with an input
        /// <see cref="PipelineStageResult"/> to produce an output <see cref="PipelineStageResult"/>.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="pipelineStepResult"></param>
        /// <returns></returns>
        protected abstract PipelineStageResult GenerateCode( PipelineStageResult input, IPipelineStepsResult pipelineStepResult );
    }
}
