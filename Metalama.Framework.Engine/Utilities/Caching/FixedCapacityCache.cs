// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Metalama.Framework.Engine.Utilities.Caching;

/// <summary>
/// A fixed-capacity cache with a LRU clean up algorithm.
/// </summary>
internal class FixedCapacityCache<T>
{
    private readonly ConcurrentDictionary<string, Entry> _cache = new( StringComparer.Ordinal );
    private long _lastUsed;
    private int _cleanUpPending;

    public int Capacity { get; }

    public int Count => this._cache.Count;

    public FixedCapacityCache( int capacity )
    {
        this.Capacity = capacity;
    }

    public Task? CleanUpTask { get; private set; }

    public T GetOrAdd( string path, Predicate<T> validate, Func<string, T> get )
    {
        if ( !this._cache.TryGetValue( path, out var entry ) || !validate( entry.Value ) )
        {
            var value = get( path );
            entry = new Entry( value );
            this._cache[path] = entry;
        }

        entry.LastUsed = Interlocked.Increment( ref this._lastUsed );

        if ( this._cache.Count > this.Capacity )
        {
            if ( Interlocked.CompareExchange( ref this._cleanUpPending, 1, 0 ) == 0 )
            {
                this.CleanUpTask = Task.Run( this.CleanUp );
            }
        }

        return entry.Value;
    }

    private void CleanUp()
    {
        while ( this._cache.Count > this.Capacity )
        {
            foreach ( var entry in this._cache )
            {
                if ( entry.Value.LastUsed <= this._lastUsed - this.Capacity )
                {
                    this._cache.TryRemove( entry.Key, out _ );
                }
            }
        }

        this._cleanUpPending = 0;
    }

    private class Entry
    {
        public long LastUsed { get; set; }

        public T Value { get; }

        public Entry( T value )
        {
            this.Value = value;
        }
    }
}