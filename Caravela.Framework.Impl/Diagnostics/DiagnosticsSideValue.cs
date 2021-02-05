using Caravela.Reactive;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Caravela.Framework.Impl.Diagnostics
{

    class DiagnosticsSideValue : IReactiveSideValue
    {
        private DiagnosticsSideValue( IImmutableList<Diagnostic>? diagnostics )
        {
            this.Diagnostics = diagnostics ?? ImmutableList<Diagnostic>.Empty;
        }

        public static DiagnosticsSideValue? Get( IReadOnlyList<Diagnostic>? list ) => list == null || list.Count == 0 ? null : new DiagnosticsSideValue( list.ToImmutableList() );
        public static DiagnosticsSideValue? Get( IImmutableList<Diagnostic>? list ) => list == null || list.Count == 0 ? null : new DiagnosticsSideValue( list );


        public IImmutableList<Diagnostic> Diagnostics { get; }

        bool IReactiveSideValue.TryCombine( IReactiveSideValue sideValue, [NotNullWhen( true )] out IReactiveSideValue? combinedValue )
        {
            if ( sideValue is DiagnosticsSideValue diagnosticsResult )
            {
                if (  diagnosticsResult.Diagnostics.Count == 0)
                {
                    combinedValue = this;
                }
                else if ( this.Diagnostics.Count == 0 )
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
