namespace Caravela.Framework.Impl
{
    internal abstract class PipelineStage
    {
        public abstract AspectCompilation Transform( AspectCompilation input );
    }
}
