// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System.Collections.Concurrent;

namespace Metalama.Framework.DesignTime.Utilities;

internal class AsyncEvent<T>
{
    private readonly ConcurrentDictionary<object, Func<T, Task>> _handlers = new();

    public void RegisterHandler( Func<T, Task> handler ) => this._handlers[handler] = handler;

    public void RegisterHandler( Action<T> handler )
        => this._handlers[handler] = arg =>
        {
            handler( arg );

            return Task.CompletedTask;
        };

    public void UnregisterHandler( Func<T, Task> handler )
    {
        this._handlers.TryRemove( handler, out _ );
    }

    public void UnregisterHandler( Action<T> handler )
    {
        this._handlers.TryRemove( handler, out _ );
    }

    public async Task InvokeAsync( T arg )
    {
        foreach ( var handler in this._handlers )
        {
            await handler.Value( arg );
        }
    }
}