using Caravela.Framework.Impl.Advices;
using Caravela.Framework.Sdk;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Caravela.Framework.Impl
{
    record AspectInstanceResult( ImmutableArray<Diagnostic> Diagnostics, ImmutableArray<AdviceInstance> Advices, ImmutableArray<AspectInstance> Aspects );
}