using Caravela.Framework.Code;
using Caravela.Framework.Diagnostics;

namespace Caravela.Framework.Workspaces
{
    /// <summary>
    /// Represents a diagnostic (error, warning, information, hidden message).
    /// </summary>
    public interface IDiagnostic
    {
        ICompilation Compilation { get; }

        string Id { get; }

        string Message { get; }

        string? FilePath { get; }

        int? Line { get; }

        IDeclaration? Declaration { get; }

        Severity Severity { get; }
    }
}