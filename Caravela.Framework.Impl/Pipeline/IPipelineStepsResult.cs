using Caravela.Framework.Aspects;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Transformations;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.Pipeline
{
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