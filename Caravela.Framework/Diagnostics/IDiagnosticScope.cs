// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;

namespace Caravela.Framework.Diagnostics
{
    /// <summary>
    /// Exposes a <see cref="DiagnosticLocation"/> property that determines the location of a user-code diagnostic.
    /// This interface is implemented by <see cref="ICodeElement"/>.
    /// </summary>
    public interface IDiagnosticScope
    {
        /// <summary>
        /// Gets the location of the current element to which diagnostics can be reported (typically the declaration name).
        /// </summary>
        IDiagnosticLocation? DiagnosticLocation { get; }
    }
}