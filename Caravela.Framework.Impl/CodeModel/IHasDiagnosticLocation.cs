// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.CodeModel
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