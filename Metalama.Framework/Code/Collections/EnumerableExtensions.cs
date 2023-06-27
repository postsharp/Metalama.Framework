// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Metalama.Framework.Aspects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Code.Collections
{
    [PublicAPI]
    [RunTimeOrCompileTime]
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> SelectRecursive<T>( T item, Func<T, T?> getNext )
            where T : class, ICompilationElement
            => SelectRecursiveInternal( item, getNext );

        internal static IEnumerable<T> SelectRecursiveInternal<T>( T item, Func<T, T?> getNext )
            where T : class
        {
            for ( var i = item; i != null; i = getNext( i ) )
            {
                yield return i;
            }
        }

        public static IEnumerable<T> SelectRecursive<T>( this IEnumerable<T> items, Func<T, T?> getNext )
            where T : class, ICompilationElement
            => items.SelectRecursiveInternal( getNext );

        internal static IEnumerable<T> SelectRecursiveInternal<T>( this IEnumerable<T> items, Func<T, T?> getNext )
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
        /// <param name="deduplicate">
        ///     <c>true</c> if duplicates should be removed from the result.
        ///     When <c>false</c>, duplicates throw in Debug build, and are not checked (causing infinite loops) in Release build.
        /// </param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        internal static IReadOnlyCollection<T> SelectManyRecursive<T>(
            this T item,
            Func<T, IEnumerable<T>?> getItems,
            bool includeThis = false,
            bool deduplicate = false )
            where T : class
        {
            var recursionCheck = 0;

#if DEBUG

            // ReSharper disable once ConvertToConstant.Local
            var useDictionary = true;
#else
            var useDictionary = deduplicate;
#endif

            if ( useDictionary )
            {
                // Create a dictionary for the results. The key is the item, the value is the order of insertion.
                Dictionary<T, int> results = new( ReferenceEqualityComparer<T>.Instance );

                if ( includeThis )
                {
                    results.Add( item, 0 );
                }

                VisitMany( getItems( item ), getItems, results, deduplicate, ref recursionCheck );

                return results.Keys;
            }
            else
            {
                // Create a list for the results.
                List<T> results = new();

                if ( includeThis )
                {
                    results.Add( item );
                }

                VisitMany( getItems( item ), getItems, results, ref recursionCheck );

                return results;
            }
        }

        /// <summary>
        /// Selects the closure of a graph. This is typically used to select all descendants of a tree node.  This method returns distinct nodes only.
        /// </summary>
        /// <param name="collection">The initial collection of items.</param>
        /// <param name="getItems">A function that returns the set of all nodes connected to a given node.</param>
        /// <param name="deduplicate">
        ///     <c>true</c> if duplicates should be removed from the result.
        ///     When <c>false</c>, duplicates throw in Debug build, and are not checked (causing infinite loops) in Release build.
        /// </param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IReadOnlyCollection<T> SelectManyRecursive<T>(
            this IEnumerable<T> collection,
            Func<T, IEnumerable<T>?> getItems,
            bool deduplicate = false )
            where T : class, ICompilationElement
            => SelectManyRecursiveInternal( collection, getItems, deduplicate );

        internal static IReadOnlyCollection<T> SelectManyRecursiveInternal<T>(
            this IEnumerable<T> collection,
            Func<T, IEnumerable<T>?> getItems,
            bool deduplicate = false )
            where T : class
        {
            var recursionCheck = 0;

#if DEBUG

            // ReSharper disable once ConvertToConstant.Local
            var useDictionary = true;
#else
            var useDictionary = deduplicate;
#endif

            if ( useDictionary )
            {
                // Create a dictionary for the results. The key is the item, the value is the order of insertion.
                Dictionary<T, int> results = new( ReferenceEqualityComparer<T>.Instance );

                VisitMany( collection, getItems, results, deduplicate, ref recursionCheck );

                return results.Keys;
            }
            else
            {
                // Create a list for the results.
                List<T> results = new();

                VisitMany( collection, getItems, results, ref recursionCheck );

                return results;
            }
        }

        private static void VisitMany<T>(
            IEnumerable<T>? collection,
            Func<T, IEnumerable<T>?> getItems,
            Dictionary<T, int> results,
            bool deduplicate,
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

                        if ( !deduplicate )
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

                    VisitMany( getItems( item ), getItems, results, deduplicate, ref recursionCheck );
                }
            }
            finally
            {
                recursionCheck--;
            }
        }

        private static void VisitMany<T>(
            IEnumerable<T>? collection,
            Func<T, IEnumerable<T>?> getItems,
            List<T> results,
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
                    results.Add( item );

                    VisitMany( getItems( item ), getItems, results, ref recursionCheck );
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

        // These exist, so that IAttributeCollection.Any overloads don't prevent usage of the Enumerable.Any overloads.
        public static bool Any( this IAttributeCollection attributes ) => Enumerable.Any( attributes );
        public static bool Any( this IAttributeCollection attributes, Func<IAttribute, bool> predicate ) => Enumerable.Any( attributes, predicate );
    }
}