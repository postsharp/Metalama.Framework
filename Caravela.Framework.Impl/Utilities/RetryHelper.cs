// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Threading;

namespace Caravela.Framework.Impl.Utilities
{
    internal static class RetryHelper
    {
        public static void Retry( Action action, Predicate<Exception> retryPredicate )
        {
            var delay = 10.0;

            for ( var i = 0; i < 8; i++ )
            {
                try
                {
                    action();
                }
                catch ( Exception e ) when ( retryPredicate( e ) )
                {
                    Thread.Sleep( TimeSpan.FromMilliseconds( delay ) );
                    delay *= 1.2;
                }
            }
        }
    }
}