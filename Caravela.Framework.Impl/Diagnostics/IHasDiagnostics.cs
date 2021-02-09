using System.Collections.Immutable;
using Caravela.Reactive;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.Diagnostics
{
    internal interface IHasDiagnostics : IHasReactiveSideValues
    {
        IImmutableList<Diagnostic> Diagnostics { get; }
    }
}
