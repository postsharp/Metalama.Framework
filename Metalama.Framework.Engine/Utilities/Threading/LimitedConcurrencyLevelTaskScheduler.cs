// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Metalama.Framework.Engine.Utilities.Threading;

// Code copied and adapted from https://learn.microsoft.com/en-us/dotnet/api/System.Threading.Tasks.TaskScheduler

internal sealed class LimitedConcurrencyLevelTaskScheduler : TaskScheduler, IDisposable
{
    public static new LimitedConcurrencyLevelTaskScheduler Default { get; } = new( Environment.ProcessorCount * 2 );

    // Indicates whether the current thread is processing work items.
    private readonly ThreadLocal<bool> _currentThreadIsProcessingItems = new();

    // The list of tasks to be executed
    private readonly LinkedList<Task> _tasks = []; // protected by lock(_tasks)

    public int PendingTasksCount => this._tasks.Count;

    // Indicates whether the scheduler is currently processing work items.
    private int _delegatesQueuedOrRunning;

    // Creates a new instance with the specified degree of parallelism.
    public LimitedConcurrencyLevelTaskScheduler( int maxDegreeOfParallelism )
    {
        if ( maxDegreeOfParallelism < 1 )
        {
            throw new ArgumentOutOfRangeException( nameof(maxDegreeOfParallelism) );
        }

        this.MaximumConcurrencyLevel = maxDegreeOfParallelism;
    }

    // Queues a task to the scheduler.
    protected override void QueueTask( Task task )
    {
        // Add the task to the list of tasks to be processed.  If there aren't enough
        // delegates currently queued or running to process tasks, schedule another.
        lock ( this._tasks )
        {
            this._tasks.AddLast( task );

            if ( this._delegatesQueuedOrRunning < this.MaximumConcurrencyLevel )
            {
                this._delegatesQueuedOrRunning++;
                this.NotifyThreadPoolOfPendingWork();
            }
        }
    }

    // Inform the ThreadPool that there's work to be executed for this scheduler.
    private void NotifyThreadPoolOfPendingWork()
    {
        ThreadPool.UnsafeQueueUserWorkItem(
            _ =>
            {
                // Note that the current thread is now processing work items.
                // This is necessary to enable inlining of tasks into this thread.
                this._currentThreadIsProcessingItems.Value = true;

                try
                {
                    // Process all available items in the queue.
                    while ( true )
                    {
                        Task item;

                        lock ( this._tasks )
                        {
                            // When there are no more items to be processed,
                            // note that we're done processing, and get out.
                            if ( this._tasks.Count == 0 )
                            {
                                this._delegatesQueuedOrRunning--;

                                break;
                            }

                            // Get the next item from the queue
                            item = this._tasks.First.AssertNotNull().Value;
                            this._tasks.RemoveFirst();
                        }

                        // Execute the task we pulled out of the queue
                        this.TryExecuteTask( item );
                    }
                }

                // We're done processing items on the current thread
                finally { this._currentThreadIsProcessingItems.Value = false; }
            },
            null );
    }

    // Attempts to execute the specified task on the current thread.
    protected override bool TryExecuteTaskInline( Task task, bool taskWasPreviouslyQueued )
    {
        // If this thread isn't already processing a task, we don't support inlining
        if ( !this._currentThreadIsProcessingItems.Value )
        {
            return false;
        }

        // If the task was previously queued, remove it from the queue
        if ( taskWasPreviouslyQueued )
        {
            // Try to run the task.

            if ( this.TryDequeue( task ) )
            {
                return this.TryExecuteTask( task );
            }
            else
            {
                return false;
            }
        }
        else
        {
            return this.TryExecuteTask( task );
        }
    }

    // Attempt to remove a previously scheduled task from the scheduler.
    protected override bool TryDequeue( Task task )
    {
        lock ( this._tasks )
        {
            return this._tasks.Remove( task );
        }
    }

    // Gets the maximum concurrency level supported by this scheduler.
    public override int MaximumConcurrencyLevel { get; }

    // Gets an enumerable of the tasks currently scheduled on this scheduler.
    protected override IEnumerable<Task> GetScheduledTasks()
    {
        var lockTaken = false;

        try
        {
            Monitor.TryEnter( this._tasks, ref lockTaken );

            if ( lockTaken )
            {
                return this._tasks;
            }
            else
            {
                throw new NotSupportedException();
            }
        }
        finally
        {
            if ( lockTaken )
            {
                Monitor.Exit( this._tasks );
            }
        }
    }

    public void Dispose() => this._currentThreadIsProcessingItems.Dispose();
}