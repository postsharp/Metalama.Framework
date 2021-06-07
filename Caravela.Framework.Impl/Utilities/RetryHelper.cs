// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Threading;

namespace Caravela.Framework.Impl.Utilities
{
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

        public static T Retry<T>( Func<T> action, Predicate<Exception>? retryPredicate = null )
        {
            var delay = 10.0;
            const int maxAttempts = 12;
            retryPredicate ??= e => e is UnauthorizedAccessException || (uint) e.HResult == 0x80070020;

            for ( var i = 0; /* nothing */; i++ )
            {
                try
                {
                    return action();
                }
                catch ( Exception e ) when ( i < maxAttempts && retryPredicate( e ) )
                {
                    Logger.Instance?.Write( $"RetryHelper caught '{e.Message}'. Retrying in {delay}." );
                    
                    Thread.Sleep( TimeSpan.FromMilliseconds( delay ) );
                    delay *= 1.2;
                }
            }
        }
    }
}