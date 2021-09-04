// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
#if !DEBUG
using System.Runtime.CompilerServices;
#endif

namespace Caravela.Framework.Impl
{
    /// <summary>
    /// A utility class that checks runtime invariant and throws <see cref="AssertionFailedException"/> in case of failure.
    /// </summary>
    [ExcludeFromCodeCoverage]
    internal static class Invariant
    {
        /// <summary>
        /// Checks that a given condition is true and throws an <see cref="AssertionFailedException"/> in case it is not.
        /// </summary>
        /// <param name="condition">The condition that must be true.</param>
        [Conditional( "DEBUG" )]
        public static void Assert( [DoesNotReturnIf( false )] bool condition )
        {
            if ( !condition )
            {
                throw new AssertionFailedException();
            }
        }

        [Conditional( "DEBUG" )]
        public static void Implies( bool premise, bool conclusion )
        {
            if ( premise && !conclusion )
            {
                throw new AssertionFailedException();
            }
        }

#if !DEBUG
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
#endif
        public static T Assert<T>( this T obj, Predicate<T> predicate )
            where T : class
        {
#if DEBUG
            if ( !predicate( obj ) )
            {
                throw new AssertionFailedException();
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
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
#endif
        public static T AssertNotNull<T>( this T? obj, string? justification = null )
            where T : class
        {
#if DEBUG
            if ( obj == null )
            {
                throw new AssertionFailedException( justification ?? $"The reference to {typeof(T).Name} must not be not null." );
            }
#endif

            return obj!;
        }

        /// <summary>
        /// Checks that a nullable value is non-null and throws an <see cref="AssertionFailedException"/> if it is not.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
#if !DEBUG
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static T AssertNotNull<T>( this T? obj, string? justification = null )
            where T : struct
        {
#if DEBUG
            if ( obj == null )
            {
                throw new AssertionFailedException( justification ?? $"The reference to {typeof(T).Name} must not be not null." );
            }
#endif

            return obj == null ? default : obj.Value;
        }

#if !DEBUG
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
#endif
        public static IEnumerable<T> AssertNoneNull<T>( this IEnumerable<T?>? items )
            where T : class
        {
#if DEBUG
            if ( items == null )
            {
                throw new AssertionFailedException( "The enumeration must not be not null." );
            }

            var i = 0;

            var list = items.ToReadOnlyList();

            foreach ( var item in list )
            {
                if ( item == null )
                {
                    throw new AssertionFailedException( $"The {i}-th {typeof(T).Name} must not be not null." );
                }

                i++;
            }

            return list!;
#else
            return items!;
#endif
        }

#if !DEBUG
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
#endif
        public static IEnumerable<T> AssertDistinct<T>( this IEnumerable<T> items )
            where T : class
        {
#if DEBUG
            var itemSet = new HashSet<T>();

            foreach ( var item in items )
            {
                if ( !itemSet.Add( item ) )
                {
                    throw new AssertionFailedException( "The enumeration must not contain equal elements." );
                }

                yield return item;
            }
#else
            return items;
#endif
        }
    }
}