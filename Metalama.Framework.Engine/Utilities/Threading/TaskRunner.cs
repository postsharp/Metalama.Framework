// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Metalama.Framework.Engine.Utilities.Threading;

#pragma warning disable VSTHRD002

internal sealed class TaskRunner : ITaskRunner
{
    private static bool MustRunNewTask() => !Thread.CurrentThread.IsBackground;

    public void RunSynchronously( Func<Task> func, CancellationToken cancellationToken = default )
    {
        if ( MustRunNewTask() )
        {
            Task.Run( func, cancellationToken ).Wait( cancellationToken );
        }
        else
        {
            func().Wait( cancellationToken );
        }
    }

    public void RunSynchronously( Func<ValueTask> func, CancellationToken cancellationToken = default )
    {
        if ( MustRunNewTask() )
        {
            Task.Run( func, cancellationToken ).Wait( cancellationToken );
        }
        else
        {
            func().GetAwaiter().GetResult();
        }
    }

    public T RunSynchronously<T>( Func<Task<T>> func, CancellationToken cancellationToken = default )
    {
        if ( MustRunNewTask() )
        {
            return Task.Run( func, cancellationToken ).Result;
        }
        else
        {
            return func().Result;
        }
    }

    public T RunSynchronously<T>( Func<ValueTask<T>> func, CancellationToken cancellationToken = default )
    {
        if ( MustRunNewTask() )
        {
            return Task.Run( () => func().AsTask(), cancellationToken ).Result;
        }
        else
        {
            return func().GetAwaiter().GetResult();
        }
    }
}