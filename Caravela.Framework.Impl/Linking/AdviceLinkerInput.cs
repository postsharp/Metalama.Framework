using System.Collections.Generic;
using Caravela.Framework.Impl.AspectOrdering;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Transformations;
using Microsoft.CodeAnalysis.CSharp;

namespace Caravela.Framework.Impl.Linking
{
    internal record AdviceLinkerInput(
        CSharpCompilation Compilation,
        CompilationModel CompilationModel,
        IReadOnlyList<INonObservableTransformation> NonObservableTransformations,
        IReadOnlyList<OrderedAspectLayer> OrderedAspectLayers
        );
}
