// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Code.Collections
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> SelectRecursive<T>( T item, Func<T, T?> getNext )
            where T : class
        {
            for ( var i = item; i != null; i = getNext( i ) )
            {
                yield return i;
            }
        }

        public static IEnumerable<T> SelectRecursive<T>( this IEnumerable<T> items, Func<T, T?> getNext )
            where T : class
        {
            foreach ( var item in items )
            {
                for ( var i = item; i != null; i = getNext( i ) )
                {
                    yield return i;
                }
            }
        }

        // NOTE: The next method is not public because it pollutes Intellisense and the documentation for all objects.

        /// <summary>
        /// Selects the closure of a graph. This is typically used to select all descendants of a tree node.  This method returns distinct nodes only.
        /// </summary>
        /// <param name="item">The initial item.</param>
        /// <param name="getItems">A function that returns the set of all nodes connected to a given node.</param>
        /// <param name="includeThis">A value indicating whether <paramref name="item"/> itself should be included in the result set.</param>
        /// <param name="throwOnDuplicate"><c>true</c> if an exception must be thrown if a duplicate if found.</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        internal static IEnumerable<T> SelectManyRecursive<T>(
            this T item,
            Func<T, IEnumerable<T>?> getItems,
            bool includeThis = false,
            bool throwOnDuplicate = true )
            where T : class
        {
            var recursionCheck = 0;

            // Create a dictionary for the results. The key is the item, the value is the order of insertion.
            Dictionary<T, int> results = new( ReferenceEqualityComparer<T>.Instance );

            if ( includeThis )
            {
                results.Add( item, 0 );
            }

            VisitMany( getItems( item ), getItems, results, throwOnDuplicate, ref recursionCheck );

            return results.Keys;
        }

        /// <summary>
        /// Selects the closure of a graph. This is typically used to select all descendants of a tree node.  This method returns distinct nodes only.
        /// </summary>
        /// <param name="collection">The initial collection of items.</param>
        /// <param name="getItems">A function that returns the set of all nodes connected to a given node.</param>
        /// <param name="throwOnDuplicate"><c>true</c> if an exception must be thrown if a duplicate if found.</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerable<T> SelectManyRecursive<T>(
            this IEnumerable<T> collection,
            Func<T, IEnumerable<T>?> getItems,
            bool throwOnDuplicate = true )
            where T : class
        {
            var recursionCheck = 0;

            // Create a dictionary for the results. The key is the item, the value is the order of insertion.
            Dictionary<T, int> results = new( ReferenceEqualityComparer<T>.Instance );

            VisitMany( collection, getItems, results, throwOnDuplicate, ref recursionCheck );

            return results.Keys;
        }

        private static void VisitMany<T>(
            IEnumerable<T>? collection,
            Func<T, IEnumerable<T>?> getItems,
            Dictionary<T, int> results,
            bool throwOnDuplicate,
            ref int recursionCheck )
            where T : class
        {
            recursionCheck++;

            try
            {
                if ( recursionCheck > 64 )
                {
                    throw new InvalidOperationException( "Too many levels of inheritance." );
                }

                if ( collection == null )
                {
                    return;
                }

                foreach ( var item in collection )
                {
                    if ( results.ContainsKey( item ) )
                    {
                        // We are in a cycle.

                        if ( throwOnDuplicate )
                        {
                            throw new InvalidOperationException( $"The item {item} of type {item.GetType().Name} has been visited twice." );
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else
                    {
                        results.Add( item, results.Count );
                    }

                    VisitMany( getItems( item ), getItems, results, throwOnDuplicate, ref recursionCheck );
                }
            }
            finally
            {
                recursionCheck--;
            }
        }

        public static IEnumerable<T> WhereNotNull<T>( this IEnumerable<T?> items )
            where T : class
            => items.Where( i => i != null )!;
    }
}