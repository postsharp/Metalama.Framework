// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Metalama.Framework.Code.Invokers
{
    /// <summary>
    /// Allows invocation of the method.
    /// </summary>
#pragma warning disable CS0612
    public interface IMethodInvoker : IInvoker
#pragma warning restore CS0612
    {
        /// <summary>
        /// Invokes the method.
        /// </summary>
        dynamic? Invoke( params dynamic?[] args );
    }
}