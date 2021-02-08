using System.Collections.Immutable;
using Caravela.Framework.Advices;
using Caravela.Framework.Sdk;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl
{
#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
    internal record AspectInstanceResult( IImmutableList<Diagnostic> Diagnostics, IImmutableList<IAdvice> Advices, IImmutableList<AspectInstance> Aspects );
#pragma warning restore SA1313 // Parameter names should begin with lower-case letter
}