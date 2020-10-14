namespace PostSharp.Framework.Impl
{
    abstract class PipelineStage
    {
        public abstract AspectCompilation Transform(AspectCompilation input);
    }
}
