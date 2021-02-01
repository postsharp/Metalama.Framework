using Caravela.Framework.Advices;
using Caravela.Framework.Impl.Advices;
using Caravela.Framework.Sdk;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Caravela.Framework.Impl
{
    record AspectInstanceResult( IImmutableList<Diagnostic> Diagnostics, IImmutableList<IAdvice> Advices, IImmutableList<AspectInstance> Aspects );
}