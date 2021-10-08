// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.Utilities
{
    /// <summary>
    /// Provides some abstraction to invoke code that require context switching, typically user code.
    /// </summary>
    internal class CodeInvoker
    {
        public IEnumerable<T> Wrap<T>( IEnumerable<T> enumerable )
        {
            var enumerator = this.Invoke( enumerable.GetEnumerator );

            while ( this.Invoke( enumerator.MoveNext ) )
            {
                yield return this.Invoke( () => enumerator.Current );
            }
        }

        public virtual T Invoke<T>( Func<T> func )
        {
            return func();
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