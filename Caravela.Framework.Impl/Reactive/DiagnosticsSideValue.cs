using Caravela.Reactive;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Caravela.Framework.Impl.Reactive
{

    class DiagnosticsSideValue : IReactiveSideValue
    {
        private DiagnosticsSideValue( ImmutableList<Diagnostic> diagnostics )
        {
            this.Diagnostics = diagnostics ?? ImmutableList<Diagnostic>.Empty;
        }

        public static DiagnosticsSideValue? Get( IReadOnlyList<Diagnostic> list ) => list == null || list.Count == 0 ? null : new DiagnosticsSideValue( list.ToImmutableList() );
        

        public ImmutableList<Diagnostic> Diagnostics { get; }

        bool IReactiveSideValue.TryCombine( IReactiveSideValue sideValue, [NotNullWhen( true )] out IReactiveSideValue? combinedValue )
        {
            if ( sideValue is DiagnosticsSideValue diagnosticsResult )
            {
                if ( diagnosticsResult.Diagnostics == null || diagnosticsResult.Diagnostics.IsEmpty)
                {
                    combinedValue = this;
                }
                else if ( this.Diagnostics == null || this.Diagnostics.IsEmpty )
                {
                    combinedValue = diagnosticsResult;
                }
                else
                {
                    combinedValue = new DiagnosticsSideValue( this.Diagnostics.AddRange( diagnosticsResult.Diagnostics ) );
                }
                return true;
            }
            else
            {
                combinedValue = null;
                return false;
            }
        }
    }
}
