// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Generic;
using Caravela.Framework.Code;

namespace Caravela.Framework.Diagnostics
{

    /// <summary>
    /// Exposes a <see cref="LocationForDiagnosticReport"/> property that determines the location of a user-code diagnostic.
    /// This interface is implemented by <see cref="ICodeElement"/>.
    /// </summary>
    public interface IDiagnosticScope
    {
        /// <summary>
        /// Gets the location of the current element to which diagnostics can be reported (typically the declaration name).
        /// </summary>
        IDiagnosticLocation? LocationForDiagnosticReport { get; }
        
        /// <summary>
        /// Gets the locations of the current element in which diagnostics can be suppressed (typically the whole declaration
        /// including trivias and attributes). In case of partial classes, this property can return several locations.
        /// </summary>
        IEnumerable<IDiagnosticLocation> LocationsForDiagnosticSuppression { get; }
    }
}