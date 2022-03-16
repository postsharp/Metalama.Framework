// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace Metalama.Framework.Engine
{
    [ExcludeFromCodeCoverage]
    public sealed class AssertionFailedException : Exception
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
                return base.ToString();
            }

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