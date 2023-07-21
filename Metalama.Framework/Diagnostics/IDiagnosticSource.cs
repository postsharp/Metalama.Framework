using Metalama.Framework.Aspects;

namespace Metalama.Framework.Diagnostics;

[CompileTime]
internal interface IDiagnosticSource
{
    string DiagnosticSourceDescription { get; }
}