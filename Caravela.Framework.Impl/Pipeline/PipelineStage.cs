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
        public IAspectPipelineOptions PipelineOptions { get; }

        protected PipelineStage( IAspectPipelineOptions pipelineOptions )
        {
            this.PipelineOptions = pipelineOptions;
        }

        /// <summary>
        /// Executes the pipeline, i.e. transforms inputs into outputs.
        /// </summary>
        /// <param name="input">The inputs.</param>
        /// <returns></returns>
        public abstract PipelineStageResult ToResult( PipelineStageResult input );
    }
}
