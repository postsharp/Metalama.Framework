namespace Caravela.Framework.Impl
{
    internal interface IAspectPipelineOptions
    {
        bool CanTransformCompilation { get; }
        bool CanAddSyntaxTrees { get; }
    }
}
