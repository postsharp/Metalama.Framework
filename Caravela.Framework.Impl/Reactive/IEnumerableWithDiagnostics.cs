using Caravela.Reactive;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Caravela.Framework.Impl.Reactive
{
    interface IEnumerableWithDiagnostics<T> : IEnumerable<T>, IHasReactiveSideValues
    {
    }

    interface IHasDiagnosticsResult : IHasReactiveSideValues
    {
        DiagnosticsResult Diagnostics { get; }
    }

    class DiagnosticsResult : IReactiveSideValue
    {
        private DiagnosticsResult( ImmutableList<Diagnostic> diagnostics )
        {
            this.Diagnostics = diagnostics ?? ImmutableList<Diagnostic>.Empty;
        }

        public static DiagnosticsResult? Get( IReadOnlyList<Diagnostic> list ) => list == null || list.Count == 0 ? null : new DiagnosticsResult( list.ToImmutableList() );
        

        public ImmutableList<Diagnostic> Diagnostics { get; }

        bool IReactiveSideValue.TryCombine( IReactiveSideValue sideValue, [NotNullWhen( true )] out IReactiveSideValue? combinedValue )
        {
            if ( sideValue is DiagnosticsResult diagnosticsResult )
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
                    combinedValue = new DiagnosticsResult( this.Diagnostics.AddRange( diagnosticsResult.Diagnostics ) );
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
