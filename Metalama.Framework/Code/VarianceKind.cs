// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Metalama.Framework.Code
{
    /// <summary>
    /// Specifies the kind variance: <see cref="In"/>, <see cref="Out"/> or <see cref="None"/>.
    /// </summary>
    public enum VarianceKind
    {
        /// <summary>
        /// No variance.
        /// </summary>
        None,

        /// <summary>
        /// Contravariant.
        /// </summary>
        In,

        /// <summary>
        /// Covariant.
        /// </summary>
        Out
    }
}