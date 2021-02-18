using System;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

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

        public InvalidUserCodeException( string message, ImmutableArray<Diagnostic> diagnostics ) : base( message )
        {
            this.Diagnostics = diagnostics;
        }

        private InvalidUserCodeException( Diagnostic diagnostic )
            : base( diagnostic.ToString() )
        {
            this.Diagnostics = ImmutableArray.Create( diagnostic );
        }
    }
}
