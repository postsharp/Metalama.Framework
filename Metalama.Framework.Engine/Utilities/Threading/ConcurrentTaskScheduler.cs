// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Metalama.Framework.Engine.Utilities.Threading;

internal class ConcurrentTaskScheduler : ITaskScheduler
{
    private LimitedConcurrencyLevelTaskScheduler _scheduler = new( Environment.ProcessorCount );

    public async Task RunInParallelAsync( IEnumerable<Action> actions, CancellationToken cancellationToken )
    {
        var tasks = new List<Task>();

        var i = 0;

        foreach ( var action in actions )
        {
            var task = Task.Factory.StartNew( action, cancellationToken, TaskCreationOptions.None, this._scheduler );
            tasks.Add( task );
        }

        await Task.WhenAll( tasks );
    }
}

internal class SingleThreadedTaskScheduler : ITaskScheduler
{
    private readonly bool _randomized;

    public SingleThreadedTaskScheduler( bool randomized )
    {
        this._randomized = randomized;
    }

    public Task RunInParallelAsync( IEnumerable<Action> actions, CancellationToken cancellationToken )
    {
        IEnumerable<Action> orderedActions;

        if ( this._randomized )
        {
            var random = new Random();
            orderedActions = actions.ToDictionary( a => a, _ => random.NextDouble() ).OrderBy( p => p.Value ).Select( p => p.Key );
        }
        else
        {
            orderedActions = actions;
        }

        try
        {
            foreach ( var action in orderedActions )
            {
                action();
            }

            return Task.CompletedTask;
        }
        catch ( Exception e )
        {
            return Task.FromException( e );
        }
    }
}