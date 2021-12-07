// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

#if NET5_0_OR_GREATER
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Caravela.Framework.RunTime
{
    /// <summary>
    /// A <see cref="List{T}"/> that implements <see cref="IAsyncEnumerable{T}"/>. This class is used when a non-iterator template is applied
    /// to an async iterator method.
    /// </summary>
    /// <typeparam name="T">Type of items.</typeparam>
    public sealed class AsyncEnumerableList<T> : List<T>, IAsyncEnumerable<T>
    {
        IAsyncEnumerator<T> IAsyncEnumerable<T>.GetAsyncEnumerator( CancellationToken cancellationToken ) => this.GetAsyncEnumerator( cancellationToken );

        /// <summary>
        /// Gets an enumerator for the current list.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public AsyncEnumerator GetAsyncEnumerator( CancellationToken cancellationToken = default ) => new( this.GetEnumerator(), cancellationToken );

        /// <summary>
        /// Implementation of <see cref="IAsyncEnumerator{T}"/>.
        /// </summary>
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