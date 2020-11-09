using Microsoft.CodeAnalysis;
using System;

namespace Caravela.Framework.Impl
{
    class CaravelaException : Exception
    {
        public Diagnostic Diagnostic{ get; }

        public CaravelaException( DiagnosticDescriptor diagnosticDescriptor, params object[] args )
            : this( diagnosticDescriptor, null, args ) { }

        public CaravelaException( DiagnosticDescriptor diagnosticDescriptor, Location? location, params object[] args )
            : this( Diagnostic.Create( diagnosticDescriptor, location, args ) ) { }

        private CaravelaException( Diagnostic diagnostic )
            : base( diagnostic.ToString() )
            => this.Diagnostic = diagnostic;
    }
}
