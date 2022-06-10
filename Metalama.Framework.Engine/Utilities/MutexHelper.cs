// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Diagnostics;

namespace Metalama.Framework.Engine.Utilities
{
    public static class MutexHelper
    {
        public static IDisposable WithGlobalLock( string name, ILogger? logger = null )
        {
            logger?.Trace?.Log( $"Acquiring lock '{name}'." );

            var mutex = CreateGlobalMutex( name, logger );

            if ( !mutex.WaitOne( 0 ) )
            {
                logger?.Trace?.Log( $"  Another process owns '{name}'. Waiting." );
                mutex.WaitOne();
            }

            logger?.Trace?.Log( $"Lock '{name}' acquired." );

            return new MutexHandle( mutex, name, logger );
        }

        private static Mutex CreateGlobalMutex( string fullName, ILogger? logger )
        {
            var mutexName = "Global\\Metalama_" + HashUtilities.HashString( fullName );

            logger?.Trace?.Log( $"  Mutex name: '{mutexName}'." );

            return new Mutex( false, mutexName );
        }

        private class MutexHandle : IDisposable
        {
            private readonly Mutex _mutex;
            private readonly string _name;
            private readonly ILogger? _logger;

            public MutexHandle( Mutex mutex, string name, ILogger? logger )
            {
                this._mutex = mutex;
                this._name = name;
                this._logger = logger;
            }

            public void Dispose()
            {
                this._logger?.Trace?.Log( $"Releasing lock '{this._name}'." );

                this._mutex.ReleaseMutex();
                this._mutex.Dispose();
            }
        }
    }
}