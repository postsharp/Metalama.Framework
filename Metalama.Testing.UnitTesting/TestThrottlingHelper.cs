// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Utilities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Metalama.Testing.UnitTesting;

internal static class TestThrottlingHelper
{
    private static readonly SemaphoreSlim _exclusiveSemaphore = new( 1 );
    private static int _runningTests;

    public static async Task<IDisposable> MonitorTestAsync( bool requiresExclusivity = false )
    {
        if ( requiresExclusivity || _exclusiveSemaphore.CurrentCount > 0 )
        {
            await _exclusiveSemaphore.WaitAsync();
        }

        Interlocked.Increment( ref _runningTests );

        return new DisposeAction( () => OnTestStopped( requiresExclusivity ) );
    }

    public static IDisposable MonitorTest( bool requiresExclusivity = false )
    {
        if ( requiresExclusivity || _exclusiveSemaphore.CurrentCount > 0 )
        {
            _exclusiveSemaphore.Wait();
        }

        Interlocked.Increment( ref _runningTests );

        return new DisposeAction( () => OnTestStopped( requiresExclusivity ) );
    }

    private static void OnTestStopped( bool requiresExclusivity )
    {
        var runningTests = Interlocked.Decrement( ref _runningTests );

        if ( requiresExclusivity || runningTests == 0 )
        {
            _exclusiveSemaphore.Release();
        }
    }
}