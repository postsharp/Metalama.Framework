﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System.Collections.Concurrent;

namespace Metalama.Framework.DesignTime.Utilities;

/// <summary>
/// Encapsulates an event where the handlers can be async and the whole event invocation is awaitable.
/// </summary>
internal sealed class AsyncEvent<T>
{
    private readonly ConcurrentDictionary<object, Func<T, Task>> _handlers = new();

    public async Task InvokeAsync( T arg )
    {
        foreach ( var handler in this._handlers )
        {
            await handler.Value( arg );
        }
    }

    public Accessors GetAccessors() => new( this );

    public readonly struct Accessors
    {
        private readonly AsyncEvent<T> _parent;

        public Accessors( AsyncEvent<T> parent )
        {
            this._parent = parent;
        }

        public void RegisterHandler( Func<T, Task> handler ) => this._parent._handlers[handler] = handler;

        public void UnregisterHandler( Func<T, Task> handler )
        {
            this._parent._handlers.TryRemove( handler, out _ );
        }
    }
}