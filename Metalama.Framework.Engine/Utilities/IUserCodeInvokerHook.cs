// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Project;
using System;
using System.Threading.Tasks;

namespace Metalama.Framework.Engine.Utilities
{
    /// <summary>
    /// Defines a method <see cref="Invoke{TResult,TPayload}"/> that is invoked when user-written code must be invoked.
    /// </summary>
    public interface IUserCodeInvokerHook : IService
    {
        /// <summary>
        /// The implementation of this method must call the delegate given delegate. It can wrap the call with any logic.
        /// </summary>
        TResult Invoke<TResult, TPayload>( UserCodeFunc<TResult, TPayload> func, ref TPayload payload );
        ValueTask<TResult> InvokeAsync<TResult>( Func<Task<TResult>> func );
    }
}