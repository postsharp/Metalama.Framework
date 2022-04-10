// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;

namespace Metalama.Framework.Code.Invokers
{
    /// <summary>
    /// Allows invocation of the method.
    /// </summary>
    [CompileTime]
    public interface IMethodInvoker : IInvoker
    {
        /// <summary>
        /// Invokes the method.
        /// </summary>
        dynamic? Invoke( dynamic? instance, params dynamic?[] args );
    }
}