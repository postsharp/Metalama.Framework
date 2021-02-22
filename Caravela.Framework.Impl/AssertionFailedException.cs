using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Caravela.Framework.Impl
{
    internal class AssertionFailedException : Exception
    {
        public IReadOnlyList<Diagnostic> Diagnostics { get; }

        public AssertionFailedException()
        {
            this.Diagnostics = Array.Empty<Diagnostic>();
        }

        public AssertionFailedException( string message ) : base( message )
        {
            this.Diagnostics = Array.Empty<Diagnostic>();
        }

        public AssertionFailedException( string message, IReadOnlyList<Diagnostic> diagnostics ) : base( GetMessage( message, diagnostics ) )
        {
            this.Diagnostics = diagnostics;
        }

        private static string GetMessage( string message, IReadOnlyList<Diagnostic> diagnostics )
            => message + Environment.NewLine + string.Join( Environment.NewLine, diagnostics.Where( d => d.Severity == DiagnosticSeverity.Error ) );

        public override string ToString()
        {
            if ( this.Diagnostics.Count == 0 )
            {
                return this.ToString();
            }
            else
            {
                StringBuilder stringBuilder = new( base.ToString() );
                stringBuilder.AppendLine( "   +----- Diagnostics " );
                foreach ( var diagnostic in this.Diagnostics )
                {
                    stringBuilder.Append( "   | " );
                    stringBuilder.AppendLine( diagnostic.ToString() );
                }

                stringBuilder.AppendLine( "   +----- " );

                return stringBuilder.ToString();
            }
        }
    }
}