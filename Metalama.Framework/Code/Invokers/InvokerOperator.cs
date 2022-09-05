// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

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