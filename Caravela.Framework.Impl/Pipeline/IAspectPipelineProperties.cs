// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Caravela.Framework.Impl.Pipeline
{
    /// <summary>
    /// Properties of an <see cref="AspectPipeline"/>.
    /// </summary>
    internal interface IAspectPipelineProperties
    {
        /// <summary>
        /// Gets a value indicating whether the pipeline can transform the Roslyn compilation. This is typically <c>false</c> for a source generator or analyzer
        /// pipeline, and <c>true</c> for a compile-time pipeline.
        /// </summary>
        bool CanTransformCompilation { get; }
    }
}