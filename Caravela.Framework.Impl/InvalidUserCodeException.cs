// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl
{
    /// <summary>
    /// An exception thrown by Caravela, embedding a <see cref="Diagnostic"/>, thrown in a situation where
    /// the responsibility can be put on the user. This exception type is typically not observed out of Caravela code,
    ///  and should be handled properly.
    /// </summary>
    internal class InvalidUserCodeException : Exception
    {
        public ImmutableArray<Diagnostic> Diagnostics { get; }

        public InvalidUserCodeException( DiagnosticDescriptor diagnosticDescriptor, params object[] args )
            : this( diagnosticDescriptor, null, args )
        {
        }

        public InvalidUserCodeException( DiagnosticDescriptor diagnosticDescriptor, Location? location, params object[] args )
            : this( Diagnostic.Create( diagnosticDescriptor, location, args ) )
        {
        }

        public InvalidUserCodeException( string message, ImmutableArray<Diagnostic> diagnostics ) : base( GetMessage( message, diagnostics ) )
        {
            this.Diagnostics = diagnostics;
        }

        private InvalidUserCodeException( Diagnostic diagnostic )
            : base( diagnostic.ToString() )
        {
            this.Diagnostics = ImmutableArray.Create( diagnostic );
        }

        private static string GetMessage( string message, IReadOnlyList<Diagnostic> diagnostics )
            => message + Environment.NewLine + string.Join( Environment.NewLine, diagnostics.Where( d => d.Severity == DiagnosticSeverity.Error ) );
    }
}
