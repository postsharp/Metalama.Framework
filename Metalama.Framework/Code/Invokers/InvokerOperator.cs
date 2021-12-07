// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Metalama.Framework.Code.Invokers
{
    /// <summary>
    /// Kinds of member access operator: <c>.</c> or <c>?.</c>.
    /// </summary>
    public enum InvokerOperator
    {
        /// <summary>
        /// Default (<c>.</c>) operator.
        /// </summary>
        Default,

        /// <summary>
        /// Null-conditional <c>'?.'</c> operator.
        /// </summary>
        Conditional
    }
}