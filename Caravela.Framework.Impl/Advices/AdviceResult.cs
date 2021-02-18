using System.Collections.Immutable;
using Caravela.Framework.Impl.Transformations;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.Advices
{
#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
    internal record AdviceResult(
        ImmutableArray<Diagnostic> Diagnostics,
        ImmutableArray<IObservableTransformation> ObservableTransformations,
        ImmutableArray<INonObservableTransformation> NonObservableTransformations );
#pragma warning restore SA1313 // Parameter names should begin with lower-case letter
}
