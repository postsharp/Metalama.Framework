namespace Caravela.Framework.Impl
{
    internal abstract class PipelineStage
    {
        public abstract PipelineStageResult ToResult( PipelineStageResult input );
    }
}
