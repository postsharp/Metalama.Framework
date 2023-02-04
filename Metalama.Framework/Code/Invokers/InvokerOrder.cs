// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using System;

namespace Metalama.Framework.Code.Invokers
{
    /// <summary>
    /// Enumeration of orders for invokers.
    /// </summary>
    [CompileTime]
    [Obsolete( "Use the RunTimeInvocationExtensions extension class.", true )]
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