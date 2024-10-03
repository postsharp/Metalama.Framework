// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine;
using System;

namespace Metalama.Testing.UnitTesting;

internal partial class TestLockingService
{
    private sealed class LockHandle
    {
        private readonly Lock _lock;
        private bool _disposed;
        private bool _acquired;

        public LockHandle( Lock @lock )
        {
            this._lock = @lock;
        }

        public void Acquire()
        {
            if ( this._acquired )
            {
                throw new InvalidOperationException();
            }

            this._lock.Wait();
            this._acquired = true;
        }

        public void Release()
        {
            if ( this._acquired )
            {
                this._lock.Release();
                this._acquired = false;
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        public void Dispose()
        {
            if ( this._acquired )
            {
                throw new InvalidOperationException( "The lock is still acquired." );
            }

            if ( !this._disposed )
            {
                this._lock.RemoveReference();
                this._disposed = true;

#if DEBUG
                GC.SuppressFinalize( this );
#endif
            }
        }

#if DEBUG
#pragma warning disable CA1821 // Remove empty Finalizers
        ~LockHandle()
        {
            throw new AssertionFailedException( "The Dispose method has not been invoked." );
        }
#pragma warning restore CA1821 // Remove empty Finalizers
#endif
    }
}