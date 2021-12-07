// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Metalama.Framework.Code.Invokers
{
    /// <summary>
    /// Enumeration of orders for invokers.
    /// </summary>
    public enum InvokerOrder
    {
        /// <summary>
        /// Final (equivalent to <c>this</c> in C#, including resolution of <c>virtual</c> calls).
        /// </summary>
        Default,

        /// <summary>
        /// Base (equivalent to <c>base</c> in C#).
        /// </summary>
        Base
    }
}