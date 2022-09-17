// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Metalama.Framework.Engine.Utilities.Threading;

internal class ConcurrentTaskScheduler : ITaskScheduler
{
    private readonly LimitedConcurrencyLevelTaskScheduler _scheduler = new( Environment.ProcessorCount );

    public async Task RunInParallelAsync<T>( IEnumerable<T> items, Action<T> action, CancellationToken cancellationToken )
    {
        var tasks = new List<Task>();

        foreach ( var item in items )
        {
            var task = Task.Factory.StartNew( () => action( item ), cancellationToken, TaskCreationOptions.None, this._scheduler );
            tasks.Add( task );
        }

        await Task.WhenAll( tasks );
    }
}