// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Diagnostics;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.Diagnostics;

/// <summary>
/// Wraps a Roslyn <see cref="Location"/> as an <see cref="IDiagnosticLocation"/>.
/// </summary>
internal sealed class LocationWrapper : IDiagnosticLocationImpl
{
    /// <summary>
    /// Gets the location.
    /// </summary>
    public Location? DiagnosticLocation { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LocationWrapper"/> class with the given <see cref="Location"/>.
    /// </summary>
    /// <param name="diagnosticLocation"></param>
    public LocationWrapper( Location? diagnosticLocation )
    {
        this.DiagnosticLocation = diagnosticLocation;
    }
}