// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Caravela.Framework.Code.Invokers
{
    public enum InvokerOrder
    {
        Default,
        Base
    }

    /// <summary>
    /// Kinds of member access operator: <c>.</c> or <c>?.</c>.
    /// </summary>
    public enum InvokerOperator
    {
        /// <summary>
        /// Default '.' operator.
        /// </summary>
        Default,

        /// <summary>
        /// Conditional ('?.') operator.
        /// </summary>
        Conditional
    }
}