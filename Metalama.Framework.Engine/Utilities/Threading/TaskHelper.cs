// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Metalama.Framework.Engine.Utilities.Threading;

#pragma warning disable VSTHRD002

public static class TaskHelper
{
    public static void RunAndWait( Func<Task> func, CancellationToken cancellationToken = default )
        => Task.Run( func, cancellationToken ).Wait( cancellationToken );

    public static void RunAndWait( Func<ValueTask> func, CancellationToken cancellationToken = default )
        => Task.Run( func, cancellationToken ).Wait( cancellationToken );

    public static T RunAndWait<T>( Func<Task<T>> func, CancellationToken cancellationToken = default ) => Task.Run( func, cancellationToken ).Result;

    public static T RunAndWait<T>( Func<ValueTask<T>> func, CancellationToken cancellationToken = default )
        => Task.Run( () => func().AsTask(), cancellationToken ).Result;
}