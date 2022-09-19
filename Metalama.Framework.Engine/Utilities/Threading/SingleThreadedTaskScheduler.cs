// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Metalama.Framework.Engine.Utilities.Threading;

public class SingleThreadedTaskScheduler : ITaskScheduler
{
    private readonly bool _randomize;

    public SingleThreadedTaskScheduler( bool randomize = false )
    {
        this._randomize = randomize;
    }

    public Task RunInParallelAsync<T>( IEnumerable<T> items, Action<T> action, CancellationToken cancellationToken )
    {
        IEnumerable<T> orderedItems;

        if ( this._randomize )
        {
            var random = new Random();
            orderedItems = items.ToDictionary( a => a, _ => random.NextDouble() ).OrderBy( p => p.Value ).Select( p => p.Key );
        }
        else
        {
            orderedItems = items;
        }

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
}