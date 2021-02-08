using System.Collections.Immutable;
using Caravela.Framework.Impl.Advices;
using Caravela.Framework.Sdk;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl
{
    internal record AspectInstanceResult( IImmutableList<Diagnostic> Diagnostics, IImmutableList<AdviceInstance> Advices, IImmutableList<AspectInstance> Aspects );
}