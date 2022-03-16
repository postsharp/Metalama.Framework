// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

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