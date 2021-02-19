using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Caravela.Framework.Impl
{
    /// <summary>
    /// A utility class that checks runtime invariant and throws <see cref="AssertionFailedException"/> in case of failure.
    /// </summary>
    internal static class Invariant
    {

        /// <summary>
        /// Checks that a given condition is true and throws an <see cref="AssertionFailedException"/> in case it is not.
        /// </summary>
        /// <param name="condition">The condition that must be true.</param>
        /// <param name="description">An error that is typically constructed as "X must be Y".</param>
        public static void Assert( [DoesNotReturnIf( false )] bool condition, string description )
        {
            if ( !condition )
            {
                throw new AssertionFailedException( "Assertion failed: " + description + "." );
            }
        }


        /// <summary>
        /// Checks that a reference is non-null and throws an <see cref="AssertionFailedException"/> if it is not.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T AssertNotNull<T>( this T? obj )
            where T : class
        {
            if ( obj == null )
            {
                throw new AssertionFailedException( $"Assertion failed: the reference to {typeof( T ).Name} must no be not null." );
            }
            
            return obj;
        }

        public static IEnumerable<T> AssertNoneNull<T>( this IEnumerable<T?>? items )
            where T : class
        {
            if ( items == null )
            {
                throw new AssertionFailedException( $"Assertion failed: the enumeration must no be not null." );
            }

            var i = 0;
            foreach ( var item in items )
            {
                if ( item == null )
                {
                    throw new AssertionFailedException( $"Assertion failed: the {i}-th {typeof(T).Name} must no be not null." );
                }

                i++;
            }
            
            return items!;
            
        }
    }
}
