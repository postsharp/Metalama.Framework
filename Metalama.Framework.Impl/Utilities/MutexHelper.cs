// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Threading;

namespace Metalama.Framework.Impl.Utilities
{
    internal static class MutexHelper
    {
        public static IDisposable WithGlobalLock( string name )
        {
            Logger.Instance?.Write( $"Acquiring lock '{name}'." );

            var mutex = CreateGlobalMutex( name );
            mutex.WaitOne();

            return new MutexHandle( mutex, name );
        }

        private static Mutex CreateGlobalMutex( string fullName )
        {
            var mutexName = "Global\\Metalama_" + HashUtilities.HashString( fullName );

            Logger.Instance?.Write( $"  Mutex name: '{mutexName}'." );

            return new Mutex( false, mutexName );
        }

        private class MutexHandle : IDisposable
        {
            private readonly Mutex _mutex;
            private readonly string _name;

            public MutexHandle( Mutex mutex, string name )
            {
                this._mutex = mutex;
                this._name = name;
            }

            public void Dispose()
            {
                Logger.Instance?.Write( $"Releasing lock '{this._name}'." );

                this._mutex.ReleaseMutex();
                this._mutex.Dispose();
            }
        }
    }
}