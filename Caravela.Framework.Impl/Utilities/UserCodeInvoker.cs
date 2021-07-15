// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Caravela.Framework.Impl.Utilities
{
    internal class UserCodeInvoker : IService
    {
        private readonly Lazy<IUserCodeInvokerHook?> _hook;

        public UserCodeInvoker( IServiceProvider serviceProvider )
        {
            // The hook is evaluated lazily so we don't get into problems of initialization order.
            this._hook = new Lazy<IUserCodeInvokerHook?>( serviceProvider.GetOptionalService<IUserCodeInvokerHook> );
        }

        public T Invoke<T>( Func<T> func )
        {
            var hook = this._hook.Value;

            if ( hook != null )
            {
                return hook.Invoke( func );
            }
            else
            {
                return func();
            }
        }

        public void Invoke( Action action )
            => this.Invoke(
                () =>
                {
                    action();

                    return true;
                } );
    }
}