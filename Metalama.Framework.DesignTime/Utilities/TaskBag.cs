using Metalama.Backstage.Diagnostics;
using System.Collections.Concurrent;

namespace Metalama.Framework.DesignTime.Utilities;

/// <summary>
/// Allows to run tasks in the background and await until all tasks have completed.
/// </summary>
public class TaskBag
{
    private int _nextId;
    private readonly ConcurrentDictionary<int, Task> _pendingTasks = new();
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

        this._pendingTasks.TryAdd( taskId, task );
    }

    public Task WaitAllAsync() => Task.WhenAll( this._pendingTasks.Values );
}