// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using System;

namespace Metalama.Framework.Code.Invokers
{
    /// <summary>
    /// Enumeration of orders for invokers.
    /// </summary>
    [CompileTime]
    [Obsolete("Use InvokerOptions", true)]
    public enum InvokerOrder
    {
        /// <summary>
        /// Equal to <see cref="Base"/>.
        /// </summary>
        Default,

        /// <summary>
        /// Accesses the implementation prior to the current aspect layer (equivalent to <c>base</c> in C#).
        /// </summary>
        Base = Default,
        
        /// <summary>
        /// Accesses the final implementation of the member.
        /// </summary>
        Final 
    }

    [CompileTime]
    [Flags]
    public enum InvokerOptions
    {
        Default,
        Base = 1,
        NullConditional = 1024
    }
}