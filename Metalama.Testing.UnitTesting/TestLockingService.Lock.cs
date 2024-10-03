// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine;
using System.Threading;

namespace Metalama.Testing.UnitTesting;

internal partial class TestLockingService
{
    private class Lock : SemaphoreSlim
    {
        private readonly TestLockingService _parent;
        private readonly string _key;

        public int References { get; private set; } = 1;

        // This locks prevents a data race between AddReference and RemoveReference. 
        // It enforces the following invariant: this.References == 0 and 'this' is not present in in this.parent.lock.
        private SpinLock _spinLock;

        public Lock( TestLockingService parent, string key ) : base( 1 )
        {
            this._parent = parent;
            this._key = key;
        }

        public Lock AddReference()
        {
            var lockTaken = false;

            try
            {
                this._spinLock.Enter( ref lockTaken );
                this.References++;
            }
            finally
            {
                if ( lockTaken )
                {
                    this._spinLock.Exit( true );
                }
            }

            return this;
        }

        public void RemoveReference()
        {
            var lockTaken = false;

            try
            {
                this._spinLock.Enter( ref lockTaken );

                this.References--;

                if ( this.References == 0 )
                {
                    if ( !this._parent._locks.TryRemove( this._key, out var removedLock ) || removedLock != this )
                    {
                        throw new AssertionFailedException( "Data race." );
                    }
                }
            }
            finally
            {
                if ( lockTaken )
                {
                    this._spinLock.Exit( true );
                }
            }
        }
    }
}