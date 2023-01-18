// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Metalama.Framework.Engine.Utilities.Threading;

public class SingleThreadedTaskRunner : IConcurrentTaskRunner
{
    public Task RunInParallelAsync<T>( IEnumerable<T> items, Action<T> action, CancellationToken cancellationToken )
        where T : notnull
    {
        var orderedItems = this.GetOrderedItems( items );

        try
        {
            foreach ( var item in orderedItems )
            {
                action( item );
            }

            return Task.CompletedTask;
        }
        catch ( Exception e )
        {
            return Task.FromException( e );
        }
    }

    public async Task RunInParallelAsync<T>( IEnumerable<T> items, Func<T, Task> action, CancellationToken cancellationToken )
        where T : notnull
    {
        var orderedItems = this.GetOrderedItems( items );

        foreach ( var item in orderedItems )
        {
            await action( item );
        }
    }

    protected virtual IEnumerable<T> GetOrderedItems<T>( IEnumerable<T> items )
        where T : notnull
        => items;
}