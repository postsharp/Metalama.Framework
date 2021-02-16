using Caravela.Framework.Diagnostics;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.Diagnostics
{
    internal class UserDiagnosticLocation : IDiagnosticLocation
    {
        public UserDiagnosticLocation( Location? location )
        {
            this.Location = location;
        }

        public Location Location { get; }

    }
}