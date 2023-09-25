// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

#if !DEBUG
using System.Runtime.CompilerServices;
#else
#endif
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Metalama.Framework.Engine
{
    /// <summary>
    /// A utility class that checks runtime invariant and throws <see cref="AssertionFailedException"/> in case of failure.
    /// </summary>
    [ExcludeFromCodeCoverage]
    [PublicAPI]
    public static class Invariant
    {
        /// <summary>
        /// Checks that a given condition is true and throws an <see cref="AssertionFailedException"/> in case it is not.
        /// </summary>
        /// <param name="condition">The condition that must be true.</param>
#if !DEBUG
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
#endif
        [DebuggerStepThrough]
        public static void Assert( [DoesNotReturnIf( false )] bool condition )
        {
#if DEBUG
            if ( !condition )
            {
                throw new AssertionFailedException();
            }
#endif
        }

        /// <summary>
        /// Checks that a given condition is false and throws an <see cref="AssertionFailedException"/> in case it is not.
        /// </summary>
        /// <param name="condition">The condition that must be true.</param>
#if !DEBUG
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
#endif
        [DebuggerStepThrough]
        public static void AssertNot( [DoesNotReturnIf( true )] bool condition )
        {
#if DEBUG
            if ( condition )
            {
                throw new AssertionFailedException();
            }
#endif
        }

#if !DEBUG
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
#endif
        [DebuggerStepThrough]
        [return: NotNullIfNotNull( "obj" )]
        public static T? AssertCast<T>( this object? obj )
            where T : class
        {
#if DEBUG
            if ( obj != null && obj is not T )
            {
                throw new AssertionFailedException( "" );
            }
#endif
            return (T?) obj;
        }

#if !DEBUG
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
#endif
        [DebuggerStepThrough]
        public static void Implies( bool premise, bool conclusion )
        {
#if DEBUG
            if ( premise && !conclusion )
            {
                throw new AssertionFailedException();
            }
#endif
        }

#if !DEBUG
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
#endif
        [DebuggerStepThrough]
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

#if !DEBUG
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
#endif
        [DebuggerStepThrough]
        public static IEnumerable<T> AssertEach<T>( this IEnumerable<T> obj, Predicate<T> predicate )
            where T : class
        {
#if DEBUG
            foreach ( var item in obj )
            {
                if ( !predicate( item ) )
                {
                    throw new AssertionFailedException();
                }

                yield return item;
            }
#else
       return obj;
#endif
        }

        /// <summary>
        /// Checks that a reference is non-null and throws an <see cref="AssertionFailedException"/> if it is not.
        /// </summary>
#if !DEBUG
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
#endif
        [DebuggerStepThrough]
        public static T AssertNotNull<T>( this T? obj, string? justification = null )
            where T : class
        {
#if DEBUG
            if ( obj == null )
            {
                throw new AssertionFailedException( justification ?? $"The reference to {typeof(T).Name} must not be not null." );
            }

            return obj;
#else
            return obj!;
#endif
        }

        /// <summary>
        /// Checks that a nullable value is non-null and throws an <see cref="AssertionFailedException"/> if it is not.
        /// </summary>
#if !DEBUG
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
#endif
        [DebuggerStepThrough]
        public static T AssertNotNull<T>( this T? obj, string? justification = null )
            where T : struct
        {
#if DEBUG
            if ( obj == null )
            {
                throw new AssertionFailedException( justification ?? $"The reference to {typeof(T).Name} must not be not null." );
            }

            return obj.Value;
#else
            return obj!.Value;
#endif
        }

#if !DEBUG
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
#endif
        [DebuggerStepThrough]
        public static IEnumerable<T> AssertNoneNull<T>( this IEnumerable<T?>? items )
            where T : class
        {
#if DEBUG
            if ( items == null )
            {
                throw new AssertionFailedException( "The enumeration must not be not null." );
            }

            var i = 0;

            foreach ( var item in items )
            {
                if ( item == null )
                {
                    throw new AssertionFailedException( $"The {i}-th {typeof(T).Name} must not be not null." );
                }

                i++;

                yield return item;
            }

#else
            return items!;
#endif
        }

#if !DEBUG
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
#endif
        [DebuggerStepThrough]
        public static ImmutableArray<T> AssertNoneNull<T>( this ImmutableArray<T?> items )
            where T : class
        {
#if DEBUG
            if ( items.IsDefault )
            {
                throw new AssertionFailedException( "The enumeration must not be not null." );
            }

            for ( var index = 0; index < items.Length; index++ )
            {
                _ = items[index]
                    ?? throw new AssertionFailedException( $"The {index}-th {typeof(T).Name} must not be not null." );
            }
#endif
            return items!;
        }

#if !DEBUG
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
#endif
        [DebuggerStepThrough]
        public static T[] AssertNoneNull<T>( this T?[]? items )
            where T : class
        {
#if DEBUG
            for ( var i = 0; i < items.AssertNotNull().Length; i++ )
            {
                if ( items![i] == null )
                {
                    throw new AssertionFailedException( $"The {i}-th {typeof(T).Name} must not be not null." );
                }
            }
#endif

            return items!;
        }

#if !DEBUG
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
#endif
        [DebuggerStepThrough]
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