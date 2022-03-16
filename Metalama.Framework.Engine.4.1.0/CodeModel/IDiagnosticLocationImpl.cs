// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Diagnostics;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.CodeModel
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