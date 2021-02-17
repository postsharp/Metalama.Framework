using Caravela.Framework.Code;

namespace Caravela.Framework.Diagnostics
{

    /// <summary>
    /// Exposes a <see cref="DiagnosticLocation"/> property that determines the location of a user-code diagnostic.
    /// This interface is implemented by <see cref="ICodeElement"/>.
    /// </summary>
    public interface IDiagnosticTarget
    {
        /// <summary>
        /// Gets the location of the current element, to which diagnostics can be emitted.
        /// </summary>
        IDiagnosticLocation? DiagnosticLocation { get; }
    }
}