using Caravela.Reactive;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Caravela.Framework.Impl.Reactive
{
    interface IHasDiagnostics : IHasReactiveSideValues
    {
        IImmutableList<Diagnostic>  Diagnostics { get; }
    }

}
