// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;

namespace Metalama.Framework.CompilerExtensions
{
    /// <summary>
    /// An implementation of a retry algorithm that does not use ILogger.
    /// </summary>
    public static class RetryHelper
    {
        public static void Retry( Action action, Predicate<Exception>? retryPredicate = null )
            => Retry(
                () =>
                {
                    action();

                    return true;
                },
                retryPredicate );

        [ExcludeFromCodeCoverage]
        public static T Retry<T>( Func<T> action, Predicate<Exception>? retryPredicate = null )
        {
            var delay = 10.0;
            const int maxAttempts = 12;
            retryPredicate ??= e => e is UnauthorizedAccessException or IOException || (uint) e.HResult == 0x80070020;

            for ( var i = 0; /* nothing */; i++ )
            {
                try
                {
                    return action();
                }
                catch ( Exception e ) when ( i < maxAttempts && retryPredicate( e ) )
                {
                    Thread.Sleep( TimeSpan.FromMilliseconds( delay ) );
                    delay *= 1.2;
                }
            }
        }
    }
}