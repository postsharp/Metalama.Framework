using Microsoft.CodeAnalysis;
using System;

namespace Caravela.Framework.Impl
{
    class CaravelaException : Exception
    {
        public Diagnostic Diagnostic{ get; }

        public CaravelaException( DiagnosticDescriptor diagnosticDescriptor, params object[] args )
            : this( Diagnostic.Create( diagnosticDescriptor, null, args ) ) { }

        private CaravelaException( Diagnostic diagnostic )
            : base( diagnostic.ToString() )
            => this.Diagnostic = diagnostic;
    }
}
