// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;
using System.Collections.Concurrent;

namespace Metalama.Framework.Engine.Utilities.Caching;

internal sealed class Pool<T>
{
    private readonly ConcurrentBag<T> _bag = new();
    private readonly Func<T> _create;

    public Pool( Func<T> create )
    {
        this._create = create;
    }

    public T Acquire()
    {
        if ( this._bag.TryTake( out var value ) )
        {
            return value;
        }
        else
        {
            return this._create();
        }
    }

    public void Release( T value )
    {
        this._bag.Add( value );
    }
}