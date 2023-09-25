// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Diagnostics;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.Diagnostics
{
    /// <summary>
    /// Exposes the Roslyn <see cref="Location"/>.
    /// </summary>
    internal interface IDiagnosticLocationImpl : IDiagnosticLocation
    {
        /// <summary>
        /// Gets the Roslyn <see cref="Location"/> of the declaration, to emit diagnostics.
        /// </summary>
        Location? DiagnosticLocation { get; }
    }
}