// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code.Collections;
using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Metalama.Framework.Engine.Utilities.Caching;

internal interface ICache<TKey, TValue>
{
    bool TryGetValue( TKey key, out TValue value );

    TValue GetOrAdd( TKey key, Func<TKey, TValue> func );

    bool TryAdd( TKey key, TValue value );
}

/// <summary>
/// A cache based on <see cref="ConditionalWeakTable{TKey,TValue}"/>, which holds a weak reference to the key.
/// </summary>
public sealed class WeakCache<TKey, TValue> : ICache<TKey, TValue>
    where TKey : class
{
    private readonly ConditionalWeakTable<TKey, StrongBox<TValue>> _cache = new();
    private readonly ConcurrentDictionary<TKey, object> _locks = new( ReferenceEqualityComparer<TKey>.Instance );
    private readonly ThreadLocal<bool> _holdsLock = new();

    public bool TryGetValue( TKey key, out TValue value )
    {
        // ReSharper disable once InconsistentlySynchronizedField
        if ( this._cache.TryGetValue( key, out var box ) )
        {
            value = box.AssertNotNull().Value!;

            return true;
        }
        else
        {
            value = default!;

            return false;
        }
    }

    public TValue GetOrAdd( TKey key, Func<TKey, TValue> func )
    {
        if ( this.TryGetValue( key, out var value ) )
        {
            return value;
        }

        if ( this._holdsLock.Value )
        {
            // This is a recursion. The thread already holds the lock.

            value = func( key );

            if ( this._cache.TryGetValue( key, out var recursiveValue ) )
            {
                return recursiveValue.Value!;
            }
            else
            {
                this._cache.Add( key, new StrongBox<TValue>( value ) );

                return value;
            }
        }
        else
        {
            // There may a race of several threads wanting to add the item to the cache.
            // We solve this problem by having a dictionary of locks. Each thread tries to have its own monitor to the dictionary.
            // The thread that wins adds the item to the cache. The other threads have to wait.

            // Create our own monitor and acquires it. We have to acquire it _before_ adding it to the dictionary of locks.
            var ourMonitor = new object();

            lock ( ourMonitor )
            {
                while ( true )
                {
                    var sharedMonitor = this._locks.GetOrAdd( key, ourMonitor );

                    if ( sharedMonitor == ourMonitor )
                    {
                        // We won the race.
                        try
                        {
                            // Create the new item.
                            try
                            {
                                this._holdsLock.Value = true;
                                value = func( key );
                            }
                            finally
                            {
                                this._holdsLock.Value = false;
                            }

                            // The func may have added the same item to the cache.
                            if ( this.TryGetValue( key, out var recursiveValue ) )
                            {
                                return recursiveValue;
                            }

                            this._cache.Add( key, new StrongBox<TValue>( value ) );

                            return value;
                        }
                        finally
                        {
                            this._locks.TryRemove( key, out _ );
                        }
                    }
                    else
                    {
                        // We lost the race, so we have to wait.
                        Monitor.Enter( sharedMonitor );
                        Monitor.Exit( sharedMonitor );

                        if ( this.TryGetValue( key, out value ) )
                        {
                            return value;
                        }
                    }
                }
            }
        }
    }

    public bool TryAdd( TKey key, TValue value )
    {
        if ( this.TryGetValue( key, out _ ) )
        {
            return false;
        }

        // There may a race of several threads wanting to add the item to the cache.
        // We solve this problem by having a dictionary of locks. Each thread tries to have its own monitor to the dictionary.
        // The thread that wins adds the item to the cache. The other threads have to wait.

        // Create our own monitor and acquires it. We have to acquire it _before_ adding it to the dictionary of locks.
        var ourMonitor = new object();

        lock ( ourMonitor )
        {
            while ( true )
            {
                var sharedMonitor = this._locks.GetOrAdd( key, ourMonitor );

                if ( sharedMonitor == ourMonitor )
                {
                    // We won the race.
                    try
                    {
                        // Create the new item.
                        this._cache.Add( key, new StrongBox<TValue>( value ) );

                        return true;
                    }
                    finally
                    {
                        this._locks.TryRemove( key, out _ );
                    }
                }
                else
                {
                    // We lost the race, but do not return until the other thread added the item.

                    Monitor.Enter( sharedMonitor );
                    Monitor.Exit( sharedMonitor );

                    return false;
                }
            }
        }
    }
}