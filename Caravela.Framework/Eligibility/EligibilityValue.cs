// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Caravela.Framework.Eligibility
{
    /// <summary>
    /// (Not implemented.)
    /// </summary>
    [Obsolete( "Not implemented." )]
    public enum EligibilityValue
    {
        // Order matters. The less eligible first.

        /// <summary>
        /// Means that the aspect can neither be applied to the target declaration nor to declarations derived from the the
        /// target declaration.
        /// </summary>
        Ineligible,

        /// <summary>
        /// Means that the aspect can be applied to declarations that are derived from the target declaration.
        /// </summary>
        EligibleForInheritanceOnly,

        /// <summary>
        /// Means that the aspect can be applied to the target declaration.
        /// </summary>
        Eligible
    }
}