using System.Collections.Generic;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Transformations;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.Linking
{
#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
    internal record AdviceLinkerInput(
        CSharpCompilation Compilation,
        CompilationModel CompilationModel,
        IReadOnlyList<INonObservableTransformation> NonObservableTransformations,
        IReadOnlyList<AspectPart> OrderedAspectParts
        );
#pragma warning restore SA1313 // Parameter names should begin with lower-case letter
}
