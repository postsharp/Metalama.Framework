using System.Collections.Immutable;
using Caravela.Reactive;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.Diagnostics
{
    internal abstract class HasDiagnostics : IHasDiagnostics
    {

        protected HasDiagnostics( IImmutableList<Diagnostic> diagnostics )
        {
            this.Diagnostics = diagnostics;
        }

        public IImmutableList<Diagnostic> Diagnostics { get; }

        ReactiveSideValues IHasReactiveSideValues.SideValues => ReactiveSideValues.Create( DiagnosticsSideValue.Get( this.Diagnostics ) );
    }
}
