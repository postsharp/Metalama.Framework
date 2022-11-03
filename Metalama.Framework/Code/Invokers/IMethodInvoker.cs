// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;

namespace Metalama.Framework.Code.Invokers
{
    /// <summary>
    /// Allows invocation of the method.
    /// </summary>
    public interface IMethodInvoker : IInvoker
    {
        /// <summary>
        /// Invokes the method.
        /// </summary>
        dynamic? Invoke( dynamic? instance, params dynamic?[] args );
    }
}