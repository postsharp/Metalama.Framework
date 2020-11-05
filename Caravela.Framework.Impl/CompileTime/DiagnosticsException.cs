using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Caravela.Framework.Impl.CompileTime
{
    class DiagnosticsException : CaravelaException
    {
        public ImmutableArray<Diagnostic> Diagnostics { get; }

        public DiagnosticsException( DiagnosticDescriptor diagnosticDescriptor, ImmutableArray<Diagnostic> diagnostics )
            : base( diagnosticDescriptor ) =>
            this.Diagnostics = diagnostics;
    }
}
