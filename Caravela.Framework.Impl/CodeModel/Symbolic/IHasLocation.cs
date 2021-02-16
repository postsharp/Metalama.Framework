using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.CodeModel.Symbolic
{
    internal interface IHasDiagnosticLocation
    {
        /// <summary>
        /// Gets the location of the code element, to emit diagnostics.
        /// </summary>
        Location? DiagnosticLocation { get; }
    }
}