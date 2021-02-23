using Caravela.Framework.Impl.AspectOrdering;
using Caravela.Framework.Impl.CodeModel;

namespace Caravela.Framework.Impl.Pipeline
{
    /// <summary>
    /// A step executed by <see cref="HighLevelPipelineStage"/>.
    /// </summary>
    internal abstract class PipelineStep
    {
      
        public PipelineStepId Id { get; }

        public OrderedAspectLayer AspectLayer { get; }

        public PipelineStep( PipelineStepId id, OrderedAspectLayer aspectLayer )
        {
            this.Id = id;
            this.AspectLayer = aspectLayer;
        }

        /// <summary>
        /// Executes the step.
        /// </summary>
        /// <param name="compilation"></param>
        /// <param name="pipelineStepsState"></param>
        /// <returns></returns>
        public abstract CompilationModel Execute( CompilationModel compilation, PipelineStepsState pipelineStepsState );
    }
}