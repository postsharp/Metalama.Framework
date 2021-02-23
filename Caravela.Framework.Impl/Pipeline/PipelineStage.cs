namespace Caravela.Framework.Impl.Pipeline
{
    /// <summary>
    /// Groups a set of transformations that, within the group, do not require the Roslyn compilation
    /// to be updated.
    /// </summary>
    internal abstract class PipelineStage
    {
        /// <summary>
        /// Gets the pipeline options.
        /// </summary>
        public IAspectPipelineProperties PipelineProperties { get; }

        protected PipelineStage( IAspectPipelineProperties pipelineProperties )
        {
            this.PipelineProperties = pipelineProperties;
        }

        /// <summary>
        /// Executes the pipeline, i.e. transforms inputs into outputs.
        /// </summary>
        /// <param name="input">The inputs.</param>
        /// <returns></returns>
        public abstract PipelineStageResult Execute( PipelineStageResult input );
    }
}
