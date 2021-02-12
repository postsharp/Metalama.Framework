using System.Collections.Immutable;
using Caravela.Framework.Advices;
using Caravela.Framework.Sdk;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace Caravela.Framework.Impl
{
#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
    internal record AspectInstanceResult( IReadOnlyList<Diagnostic> Diagnostics, IReadOnlyList<IAdvice> Advices, IReadOnlyList<IAspectSource> AspectSources );
#pragma warning restore SA1313 // Parameter names should begin with lower-case letter
}