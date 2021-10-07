// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Project;
using System;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.Utilities
{
    internal class UserCodeInvoker : IService
    {
        private readonly IUserCodeInvokerHook? _hook;

        public UserCodeInvoker( IServiceProvider? serviceProvider )
        {
            this._hook = serviceProvider?.GetOptionalService<IUserCodeInvokerHook>();
        }

        public IEnumerable<T> Wrap<T>( IEnumerable<T> enumerable )
        {
            var enumerator = this.Invoke( enumerable.GetEnumerator );

            while ( this.Invoke( enumerator.MoveNext ) )
            {
                yield return this.Invoke( () => enumerator.Current );
            }
        }

        public T Invoke<T>( Func<T> func )
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

        public void Invoke( Action action )
            => this.Invoke(
                () =>
                {
                    action();

                    return true;
                } );
    }
}