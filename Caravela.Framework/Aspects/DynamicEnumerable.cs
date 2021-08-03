// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Caravela.Framework.Aspects
{
    internal class DynamicEnumerable : IEnumerable<object>, IEnumerator<object>
#if NET5_0
                                     , IAsyncEnumerable<object>, IAsyncEnumerator<object>
#endif
    {
        public object? Expression { get; }

        public DynamicEnumerable( object? expression )
        {
            this.Expression = expression;
        }

        public IEnumerator<object> GetEnumerator() => this;

        IEnumerator IEnumerable.GetEnumerator() => this;

        public bool MoveNext() => throw new NotSupportedException();

        public void Reset() => throw new NotSupportedException();

        public object Current => throw new NotSupportedException();

        public void Dispose() { }

#if NET5_0
        public ValueTask<bool> MoveNextAsync() => throw new NotSupportedException();

        public IAsyncEnumerator<object> GetAsyncEnumerator( CancellationToken cancellationToken = default ) => this;

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
#endif
    }
}