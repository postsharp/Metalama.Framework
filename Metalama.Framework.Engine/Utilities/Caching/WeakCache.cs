// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Metalama.Framework.Engine.Utilities.Caching;

/// <summary>
/// A cache based on <see cref="ConditionalWeakTable{TKey,TValue}"/>, which holds a weak reference to the key.
/// </summary>
public readonly struct WeakCache<TKey, TValue> 
    where TKey : class
{
    private readonly ConditionalWeakTable<TKey, StrongBox<TValue>> _cache = new();

    public WeakCache() { }

    public bool TryGetValue( TKey key, out TValue value )
    {
        // ReSharper disable once InconsistentlySynchronizedField
        if ( this._cache.TryGetValue( key, out var box ) )
        {
            value = box.Value;

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
        else
        {
            lock ( this._cache )
            {
                if ( this.TryGetValue( key, out value ) )
                {
                    return value;
                }
                else
                {
                    value = func( key );

                    // In case the func() implementation added the value, return it.
                    if ( this.TryGetValue( key, out var value2 ) )
                    {
                        return value2;
                    }

                    this._cache.Add( key, new StrongBox<TValue>( value ) );

                    return value;
                }
            }
        }
    }

    public bool TryAdd( TKey key, TValue value )
    {
        lock ( this._cache )
        {
            if ( this._cache.TryGetValue( key, out _ ) )
            {
                return false;
            }
            else
            {
                this._cache.Add( key, new StrongBox<TValue>( value ) );

                return true;
            }
        }
    }

    public TValue GetOrAdd( TKey key, Func<TKey, CancellationToken, TValue> func, CancellationToken cancellationToken )
    {
        if ( this.TryGetValue( key, out var value ) )
        {
            return value;
        }
        else
        {
            lock ( this._cache )
            {
                if ( this.TryGetValue( key, out value ) )
                {
                    return value;
                }
                else
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    value = func( key, cancellationToken );

                    this._cache.Add( key, new StrongBox<TValue>( value ) );

                    return value;
                }
            }
        }
    }
}