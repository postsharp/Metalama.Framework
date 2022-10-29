// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Diagnostics;
using System.Collections.Concurrent;

namespace Metalama.Framework.DesignTime.Utilities;

/// <summary>
/// Allows to run tasks in the background and await until all tasks have completed.
/// </summary>
public class TaskBag
{
    private int _nextId;
    private readonly ConcurrentDictionary<int, (Task Task, Func<Task> Func)> _pendingTasks = new();
    private readonly ILogger _logger;

    public TaskBag( ILogger logger )
    {
        this._logger = logger;
    }

    public void Run( Func<Task> asyncAction, CancellationToken cancellationToken = default )
    {
        var taskId = Interlocked.Increment( ref this._nextId );

        var task = Task.Run(
            async () =>
            {
                try
                {
                    await asyncAction();
                }
                catch ( Exception e )
                {
                    DesignTimeExceptionHandler.ReportException( e, this._logger );
                }
                finally
                {
                    this._pendingTasks.TryRemove( taskId, out _ );
                }
            },
            cancellationToken );

        this._pendingTasks.TryAdd( taskId, (task, asyncAction) );
    }

    public async Task WaitAllAsync()
    {
        var delay5 = Task.Delay( 5000 );

        if ( await Task.WhenAny( delay5, Task.WhenAll( this._pendingTasks.Values.Select( x => x.Task ) ) ) == delay5 )
        {
            this._logger.Warning?.Log(
                "The following tasks take a long time to complete: " + string.Join( ", ", this._pendingTasks.Select( x => x.Value.Func.ToString() ) ) );
        }

        // Avoid blocking forever in case of bug.

        var delay30 = Task.Delay( 30000 );

        if ( await Task.WhenAny( delay30, Task.WhenAll( this._pendingTasks.Values.Select( x => x.Task ) ) ) == delay30 )
        {
            throw new TimeoutException(
                "The following tasks did not complete complete in time: " + string.Join( ", ", this._pendingTasks.Select( x => x.Value.Func.ToString() ) ) );
        }
    }
}