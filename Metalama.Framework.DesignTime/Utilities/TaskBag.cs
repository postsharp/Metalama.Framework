// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Diagnostics;
using System.Collections.Concurrent;

namespace Metalama.Framework.DesignTime.Utilities;

/// <summary>
/// Allows to run tasks in the background and await until all tasks have completed.
/// </summary>
public class TaskBag
{
    private readonly ConcurrentDictionary<int, (Task Task, Func<Task> Func)> _pendingTasks = new();
    private readonly ILogger _logger;
    private int _nextId;

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
        var shortDelay = Task.Delay( 5_000 );

        if ( await Task.WhenAny( shortDelay, Task.WhenAll( this._pendingTasks.Values.Select( x => x.Task ) ) ) == shortDelay )
        {
            this._logger.Warning?.Log(
                "The following tasks take a long time to complete: " + string.Join( ", ", this._pendingTasks.SelectEnumerable( x => x.Value.Func.ToString() ) ) );
        }

        // Avoid blocking forever in case of bug.

        var longDelay = Task.Delay( 180_000 );

        if ( await Task.WhenAny( longDelay, Task.WhenAll( this._pendingTasks.Values.Select( x => x.Task ) ) ) == longDelay )
        {
            throw new TimeoutException(
                "The following tasks did not complete complete in time: " + string.Join(
                    ", ",
                    this._pendingTasks.SelectEnumerable( x => x.Value.Func.Method.ToString() ) ) );
        }
    }
}