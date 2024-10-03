// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Framework.Engine;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Utilities.Threading;
using System;
using System.Collections.Concurrent;

namespace Metalama.Testing.UnitTesting;

// This implementation was copied from Metalama.Patterns.Caching.

internal partial class TestLockingService : ILockingService
{
    private readonly ConcurrentDictionary<string, Lock> _locks = new( StringComparer.OrdinalIgnoreCase );

    private LockHandle GetLock( string key )
    {
        var @lock = this._locks.AddOrUpdate( key, k => new Lock( this, k ), ( _, l ) => l.AddReference() );
#if DEBUG
        if ( @lock.References <= 0 )
        {
            throw new AssertionFailedException();
        }
#endif
        return new LockHandle( @lock );
    }

    public IDisposable WithLock( string name, ILogger? logger = null )
    {
        var handle = this.GetLock( name );
        handle.Acquire();

        return new DisposeAction(
            () =>
            {
                handle.Release();
                handle.Dispose();
            } );
    }
}