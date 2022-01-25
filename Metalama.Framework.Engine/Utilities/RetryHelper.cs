// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Diagnostics;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;

namespace Metalama.Framework.Engine.Utilities
{
    public static class RetryHelper
    {
        public static void Retry( Action action, Predicate<Exception>? retryPredicate = null, ILogger? logger = null )
            => Retry(
                () =>
                {
                    action();

                    return true;
                },
                retryPredicate,
                logger );

        [ExcludeFromCodeCoverage]
        public static T Retry<T>( Func<T> action, Predicate<Exception>? retryPredicate = null, ILogger? logger = null )
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
                    logger?.Warning?.Log( $"RetryHelper caught {e.GetType().Name} '{e.Message}'. Retrying in {delay}." );

                    Thread.Sleep( TimeSpan.FromMilliseconds( delay ) );
                    delay *= 1.2;
                }
            }
        }
    }
}