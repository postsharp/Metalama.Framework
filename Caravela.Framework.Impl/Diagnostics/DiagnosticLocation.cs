using Caravela.Framework.Diagnostics;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.Diagnostics
{
    /// <summary>
    /// Wraps a Roslyn <see cref="Location"/> into a <see cref="DiagnosticLocation"/>.
    /// </summary>
    internal class DiagnosticLocation : IDiagnosticLocation
    {
        public DiagnosticLocation( Location? location )
        {
            this.Location = location;
        }

        public Location? Location { get; }
    }
}