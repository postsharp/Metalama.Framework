// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Utilities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Metalama.Testing.AspectTesting;

public static class TestThrottlingHelper
{
    private static readonly SemaphoreSlim _concurrentSemaphore = new( Environment.ProcessorCount );
    private static readonly SemaphoreSlim _exclusiveSemaphore = new( 1 );
    private static int _runningTests;

    public static async Task<IDisposable> ThrottleAsync()
    {
        await _concurrentSemaphore.WaitAsync();

        if ( Interlocked.Increment( ref _runningTests ) == 1 )
        {
            await _exclusiveSemaphore.WaitAsync();
        }

        return new DisposeAction(
            () =>
            {
                _concurrentSemaphore.Release();

                if ( Interlocked.Decrement( ref _runningTests ) == 0 )
                {
                    _exclusiveSemaphore.Release();
                }
            } );
    }

    public static async Task<IDisposable> RequiresExclusivityAsync()
    {
        await _exclusiveSemaphore.WaitAsync();

        return new DisposeAction(
            () =>
            {
                _exclusiveSemaphore.Release();
            } );
    }
}