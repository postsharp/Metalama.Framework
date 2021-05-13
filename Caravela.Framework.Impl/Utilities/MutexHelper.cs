// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Threading;

namespace Caravela.Framework.Impl.Utilities
{
    internal static class MutexHelper
    {
        public static Mutex CreateGlobalMutex( string fullName )
        {
            var mutexName = "Global\\Caravela_" + HashUtilities.HashString( fullName );

            return new Mutex( false, mutexName );
        }
    }
}