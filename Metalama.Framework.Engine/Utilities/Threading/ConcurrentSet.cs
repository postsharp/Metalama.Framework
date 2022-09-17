// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.Utilities.Threading;

internal readonly struct ConcurrentSet<T> : IReadOnlySet<T>
{
    private readonly ConcurrentDictionary<T, int> _dictionary = new();

    public ConcurrentSet()
    {
        
    }

    public bool Add( T value ) => this._dictionary.TryAdd( value, 0 );

    public bool Contains( T item ) => this._dictionary.ContainsKey( item );

    public IEnumerator<T> GetEnumerator() => this._dictionary.Keys.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

    public int Count => this._dictionary.Count;
}