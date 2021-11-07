// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Project;
using System;

namespace Caravela.Framework.Impl.Utilities
{
    /// <summary>
    /// An implementation of <see cref="CodeInvoker"/> that switches the <see cref="CaravelaExecutionContextImpl"/>
    /// and optionally invokes an <see cref="IUserCodeInvokerHook"/> (this hook is used by Try Caravela).
    /// </summary>
    internal class UserCodeInvoker : CodeInvoker, IService
    {
        private readonly IUserCodeInvokerHook? _hook;

        public UserCodeInvoker( IServiceProvider serviceProvider )
        {
            this._hook = serviceProvider.GetOptionalService<IUserCodeInvokerHook>();
        }

        public override T Invoke<T>( Func<T> func )
        {
            if ( this._hook != null )
            {
                return this._hook.Invoke( func );
            }
            else
            {
                return func();
            }
        }
    }
}