// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Metalama.Framework.Engine.Utilities.Threading;

internal sealed class ConcurrentTaskRunner : IConcurrentTaskRunner, IDisposable
{
    private readonly LimitedConcurrencyLevelTaskScheduler _scheduler = new( Environment.ProcessorCount );

    public async Task RunInParallelAsync<T>( IEnumerable<T> items, Action<T> action, CancellationToken cancellationToken )
        where T : notnull
    {
        // Enqueue all items.
        var queue = new ConcurrentQueue<T>();

        foreach ( var item in items )
        {
            queue.Enqueue( item );
        }

        // Start tasks to process the queue.
        var taskCount = Math.Min( Environment.ProcessorCount, queue.Count );
        var tasks = new Task[taskCount];

        for ( var i = 0; i < taskCount; i++ )
        {
            tasks[i] = Task.Factory.StartNew( ProcessQueue, cancellationToken, TaskCreationOptions.None, this._scheduler );
        }

        // Await all tasks.
        await Task.WhenAll( tasks );

        // Process the queue.
        void ProcessQueue()
        {
            while ( queue.TryDequeue( out var item ) )
            {
                cancellationToken.ThrowIfCancellationRequested();
                action( item );
            }
        }
    }

    public async Task RunInParallelAsync<TItem, TContext>(
        IEnumerable<TItem> items,
        Action<TItem, TContext> action,
        Func<TContext> createContext,
        CancellationToken cancellationToken )
        where TItem : notnull
        where TContext : IDisposable
    {
        // Enqueue all items.
        var queue = new ConcurrentQueue<TItem>();

        foreach ( var item in items )
        {
            queue.Enqueue( item );
        }

        // Start tasks to process the queue.
        var taskCount = Math.Min( Environment.ProcessorCount, queue.Count );
        var tasks = new Task[taskCount];

        for ( var i = 0; i < taskCount; i++ )
        {
            tasks[i] = Task.Factory.StartNew( ProcessQueue, cancellationToken, TaskCreationOptions.None, this._scheduler );
        }

        // Await all tasks.
        await Task.WhenAll( tasks );

        // Process the queue.
        void ProcessQueue()
        {
            using var context = createContext();

            while ( queue.TryDequeue( out var item ) )
            {
                cancellationToken.ThrowIfCancellationRequested();
                action( item, context );
            }
        }
    }

    public async Task RunInParallelAsync<T>( IEnumerable<T> items, Func<T, Task> action, CancellationToken cancellationToken )
        where T : notnull
    {
        var queue = new ConcurrentQueue<T>();

        foreach ( var item in items )
        {
            queue.Enqueue( item );
        }

        var taskCount = Math.Min( Environment.ProcessorCount, queue.Count );
        var tasks = new Task[taskCount];

        for ( var i = 0; i < taskCount; i++ )
        {
            tasks[i] = Task.Factory.StartNew( ProcessQueueAsync, cancellationToken, TaskCreationOptions.None, this._scheduler ).Unwrap();
        }

        await Task.WhenAll( tasks );

        async Task ProcessQueueAsync()
        {
            while ( queue.TryDequeue( out var item ) )
            {
                cancellationToken.ThrowIfCancellationRequested();
                await action( item );
            }
        }
    }

    public void Dispose() => this._scheduler.Dispose();
}