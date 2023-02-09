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

    public static async Task<IDisposable> StartTestAsync( bool requiresExclusivity = false )
    {
        if ( requiresExclusivity || Interlocked.Increment( ref _runningTests ) == 1 )
        {
            await _exclusiveSemaphore.WaitAsync();
        }

        return new DisposeAction(
            () =>
            {
                if ( requiresExclusivity || Interlocked.Decrement( ref _runningTests ) == 0 )
                {
                    _exclusiveSemaphore.Release();
                }
            } );
    }

    public static IDisposable StartTest( bool requiresExclusivity = false )
    {
        if ( requiresExclusivity || Interlocked.Increment( ref _runningTests ) == 1 )
        {
            _exclusiveSemaphore.Wait();
        }

        return new DisposeAction(
            () =>
            {
                if ( requiresExclusivity || Interlocked.Decrement( ref _runningTests ) == 0 )
                {
                    _exclusiveSemaphore.Release();
                }
            } );
    }
}