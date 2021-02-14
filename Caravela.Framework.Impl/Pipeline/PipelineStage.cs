namespace Caravela.Framework.Impl
{
    internal abstract class PipelineStage
    {
        public IAspectPipelineOptions PipelineOptions { get; }

        public abstract PipelineStageResult ToResult( PipelineStageResult input );
    }
}
