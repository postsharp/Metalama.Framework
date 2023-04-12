// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.CodeModel;

internal sealed class LocationWrapper : IDiagnosticLocationImpl
{
    public Location? DiagnosticLocation { get; }

    public LocationWrapper( Location? diagnosticLocation )
    {
        this.DiagnosticLocation = diagnosticLocation;
    }
}