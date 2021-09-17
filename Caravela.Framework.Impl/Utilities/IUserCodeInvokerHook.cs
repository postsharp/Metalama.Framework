// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.ServiceProvider;
using System;

namespace Caravela.Framework.Impl.Utilities
{
    /// <summary>
    /// Defines a method <see cref="Invoke{T}"/> that is invoked when user-written code must be invoked.
    /// </summary>
    public interface IUserCodeInvokerHook : IService
    {
        /// <summary>
        /// The implementation of this method must call the delegate given delegate. It can wrap the call with any logic.
        /// </summary>
        T Invoke<T>( Func<T> func );
    }
}