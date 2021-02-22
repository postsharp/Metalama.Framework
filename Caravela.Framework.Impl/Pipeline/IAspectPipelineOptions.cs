namespace Caravela.Framework.Impl.Pipeline
{
    /// <summary>
    /// Properties of an <see cref="AspectPipeline"/>.
    /// </summary>
    internal interface IAspectPipelineProperties
    {
        /// <summary>
        /// Determines whether the pipeline can transform the Roslyn compilation. This is typically <c>false</c> for a source generator or analyzer
        /// pipeline, and <c>true</c> for a compile-time pipeline.
        /// </summary>
        bool CanTransformCompilation { get; }
    }
}
