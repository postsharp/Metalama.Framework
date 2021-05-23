using Caravela.Framework.Diagnostics;

namespace Caravela.Framework.Validation
{
    public readonly struct ValidateDeclarationContext<T>
    {
        IDiagnosticSink Diagnostics { get; }
        T Declaration { get; }
    }
}