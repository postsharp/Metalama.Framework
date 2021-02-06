using System.Collections.Immutable;
using Caravela.Framework.Impl.Transformations;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.Advices
{
    internal record AdviceResult( ImmutableArray<Diagnostic> Diagnostics, ImmutableArray<Transformation> Transformations );
}
