namespace Caravela.Framework.Impl
{
    abstract class PipelineStage
    {
        public abstract PipelineStageResult ToResult( PipelineStageResult input );
    }
}
