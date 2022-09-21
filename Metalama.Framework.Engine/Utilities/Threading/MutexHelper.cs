// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Diagnostics;
using System;
using System.Threading;

#if DEBUG
using System.Diagnostics;
#endif

namespace Metalama.Framework.Engine.Utilities.Threading
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

#if DEBUG
            private readonly StackTrace _stackTrace = new();
#endif

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

#if DEBUG
                GC.SuppressFinalize( this );
#endif
            }

#pragma warning disable CA1821
#if DEBUG
            ~MutexHandle()
            {
                throw new AssertionFailedException( "The mutex was not disposed. It was acquired here: " + Environment.NewLine + this._stackTrace );
            }
#endif
#pragma warning restore CA1821
        }
    }
}