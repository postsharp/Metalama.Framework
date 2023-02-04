// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;

namespace Metalama.Framework.Code.Invokers
{
    /// <summary>
    /// Allows invocation of the method.
    /// </summary>
    [Obsolete( "Use the RunTimeInvocationExtensions extension class.", true )]
    public interface IMethodInvoker : IInvoker
    {
        /// <summary>
        /// Invokes the method.
        /// </summary>
        dynamic? Invoke( dynamic? target, params dynamic?[] args );
    }
}