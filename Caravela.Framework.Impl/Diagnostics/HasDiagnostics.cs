using Caravela.Reactive;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

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
