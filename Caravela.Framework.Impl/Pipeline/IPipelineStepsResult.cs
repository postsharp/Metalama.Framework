using System.Collections.Generic;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Transformations;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.Pipeline
{
    /// <summary>
    /// A read-only view of <see cref="PipelineStepsState"/>/
    /// </summary>
    internal interface IPipelineStepsResult
    {
        CompilationModel Compilation { get; }

        IReadOnlyList<INonObservableTransformation> NonObservableTransformations { get; }

        IReadOnlyList<Diagnostic> Diagnostics { get; }

        /// <summary>
        /// Gets the list of aspect sources that are not a part of the current pipeline stage.
        /// </summary>
        IReadOnlyList<IAspectSource> ExternalAspectSources { get; }
    }
}