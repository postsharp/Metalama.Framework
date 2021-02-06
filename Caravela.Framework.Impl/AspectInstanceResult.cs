using System.Collections.Immutable;
using Caravela.Framework.Advices;
using Caravela.Framework.Sdk;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl
{
    internal record AspectInstanceResult( IImmutableList<Diagnostic> Diagnostics, IImmutableList<IAdvice> Advices, IImmutableList<AspectInstance> Aspects );
}