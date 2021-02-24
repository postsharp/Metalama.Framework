// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;

namespace Caravela.Framework.Diagnostics
{

    /// <summary>
    /// Exposes a <see cref="DiagnosticLocation"/> property that determines the location of a user-code diagnostic.
    /// This interface is implemented by <see cref="ICodeElement"/>.
    /// </summary>
    public interface IDiagnosticTarget
    {
        /// <summary>
        /// Gets the location of the current element, to which diagnostics can be emitted.
        /// </summary>
        IDiagnosticLocation? DiagnosticLocation { get; }
    }
}