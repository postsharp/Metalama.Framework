// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Threading;

namespace Caravela.Framework.Impl.Utilities
{
    internal static class MutexHelper
    {
        public static IDisposable WithGlobalLock( string name )
        {
            var mutex = CreateGlobalMutex( name );
            mutex.WaitOne();

            return new MutexHandle( mutex );
        }

        private static Mutex CreateGlobalMutex( string fullName )
        {
            var mutexName = "Global\\Caravela_" + HashUtilities.HashString( fullName );

            return new Mutex( false, mutexName );
        }

        private class MutexHandle : IDisposable
        {
            private readonly Mutex _mutex;

            public MutexHandle( Mutex mutex )
            {
                this._mutex = mutex;
            }

            public void Dispose()
            {
                this._mutex.ReleaseMutex();
                this._mutex.Dispose();
            }
        }
    }
}