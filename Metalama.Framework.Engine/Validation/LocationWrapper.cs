using Metalama.Framework.Engine.CodeModel;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.Validation;

internal class LocationWrapper : IDiagnosticLocationImpl
{
    public Location? DiagnosticLocation { get; }

    public LocationWrapper( Location? diagnosticLocation )
    {
        this.DiagnosticLocation = diagnosticLocation;
    }
}