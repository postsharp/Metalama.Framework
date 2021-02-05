using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.CompileTime
{
    internal class DiagnosticsException : CaravelaException
    {
        public ImmutableArray<Diagnostic> Diagnostics { get; }

        public DiagnosticsException( DiagnosticDescriptor diagnosticDescriptor, ImmutableArray<Diagnostic> diagnostics )
            : base( diagnosticDescriptor ) =>
            this.Diagnostics = diagnostics;
    }
}
