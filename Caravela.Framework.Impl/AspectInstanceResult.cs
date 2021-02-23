using System.Collections.Generic;
using Caravela.Framework.Advices;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl
{
    internal record AspectInstanceResult(
        bool Success,
        IReadOnlyList<Diagnostic> Diagnostics,
        IReadOnlyList<IAdvice> Advices,
        IReadOnlyList<IAspectSource> AspectSources );
}