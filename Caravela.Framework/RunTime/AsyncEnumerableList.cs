// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

#if NET5_0
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Caravela.Framework.RunTime
{
    public sealed class AsyncEnumerableList<T> : List<T>, IAsyncEnumerable<T>
    {
        IAsyncEnumerator<T> IAsyncEnumerable<T>.GetAsyncEnumerator( CancellationToken cancellationToken ) => this.GetAsyncEnumerator( cancellationToken );

        public AsyncEnumerator GetAsyncEnumerator( CancellationToken cancellationToken = default )
            => new AsyncEnumerator( this.GetEnumerator(), cancellationToken );

        public struct AsyncEnumerator : IAsyncEnumerator<T>
        {
            private readonly CancellationToken _cancellationToken;
            private Enumerator _enumerator;

            public AsyncEnumerator( in Enumerator enumerator, CancellationToken cancellationToken )
            {
                this._enumerator = enumerator;
                this._cancellationToken = cancellationToken;
            }

            public ValueTask DisposeAsync() => ValueTask.CompletedTask;

            public ValueTask<bool> MoveNextAsync()
            {
                this._cancellationToken.ThrowIfCancellationRequested();

                return ValueTask.FromResult( this._enumerator.MoveNext() );
            }

            public T Current => this._enumerator.Current;
        }
    }
}

#endif