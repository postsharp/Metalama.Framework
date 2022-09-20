// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Metalama.Framework.Engine.Utilities.Threading;

public class SingleThreadedTaskScheduler : ITaskScheduler
{
    public SingleThreadedTaskScheduler() { }

    public Task RunInParallelAsync<T>( IEnumerable<T> items, Action<T> action, CancellationToken cancellationToken )
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

    protected virtual IEnumerable<T> GetOrderedItems<T>( IEnumerable<T> items ) => items;
}