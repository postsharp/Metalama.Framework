using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.CodeModel.Symbolic
{
    /// <summary>
    /// Exposes the Roslyn <see cref="Location"/>.
    /// </summary>
    internal interface IHasDiagnosticLocation
    {
        /// <summary>
        /// Gets the Roslyn <see cref="Location"/> of the code element, to emit diagnostics.
        /// </summary>
        Location? DiagnosticLocation { get; }
    }
}