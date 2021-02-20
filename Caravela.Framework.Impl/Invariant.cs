using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

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
        [Conditional("DEBUG")]
        public static void Assert( [DoesNotReturnIf( false )] bool condition )
        {
            if ( !condition )
            {
                throw new AssertionFailedException( );
            }
        }

        [Conditional("DEBUG")]
        public static void Implies( bool premise, bool conclusion )
        {
            if ( premise && !conclusion )
            {
                throw new AssertionFailedException( );
            }
        }


#if !DEBUG
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static T Assert<T>( this T obj, Predicate<T> predicate )
            where T : class
        {
#if DEBUG
            if ( !predicate(obj) )
            {
                throw new AssertionFailedException(  );
            }
#endif

            return obj;
        }


        /// <summary>
        /// Checks that a reference is non-null and throws an <see cref="AssertionFailedException"/> if it is not.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
#if !DEBUG
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif 
        public static T AssertNotNull<T>( this T? obj )
            where T : class
        {
#if DEBUG            
            if ( obj == null )
            {
                throw new AssertionFailedException( $"The reference to {typeof( T ).Name} must no be not null." );
            }
#endif
            
            return obj;
        }

#if !DEBUG
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif         
        public static IEnumerable<T> AssertNoneNull<T>( this IEnumerable<T?>? items )
            where T : class
        {
#if DEBUG
            if ( items == null )
            {
                throw new AssertionFailedException( $"The enumeration must no be not null." );
            }

            var i = 0;
            foreach ( var item in items )
            {
                if ( item == null )
                {
                    throw new AssertionFailedException( $"The {i}-th {typeof(T).Name} must no be not null." );
                }

                i++;
            }
#endif
            
            return items!;
            
        }
    }
}
