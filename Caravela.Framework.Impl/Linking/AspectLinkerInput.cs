using System.Collections.Generic;
using Caravela.Framework.Impl.CodeModel.Symbolic;
using Caravela.Framework.Impl.Transformations;
using Microsoft.CodeAnalysis.CSharp;

namespace Caravela.Framework.Impl.Linking
{
    internal record AspectLinkerInput(
        CSharpCompilation Compilation,
        CompilationModel CompilationModel,
        IReadOnlyList<INonObservableTransformation> NonObservableTransformations,
        IReadOnlyList<AspectPart> OrderedAspectParts
        );
}
