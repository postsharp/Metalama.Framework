// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;

namespace Caravela.Framework.Code.Invokers
{
    /// <summary>
    /// Allows invocation of the method.
    /// </summary>
    [CompileTimeOnly]
    public interface IMethodInvoker : IInvoker
    {
        /// <summary>
        /// Invokes the method.
        /// </summary>
        dynamic? Invoke( dynamic? instance, params dynamic?[] args );
    }
}