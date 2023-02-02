using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.Diagnostics;

public sealed class ThrowingDiagnosticAdder : IDiagnosticAdder
{
    public void Report( Diagnostic diagnostic )
    {
        if ( diagnostic.Severity == DiagnosticSeverity.Error )
        {
            throw new DiagnosticException( diagnostic );
        }
    }
}