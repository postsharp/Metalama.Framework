// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Caravela.Framework.Impl.Collections
{
    /// <summary>
    /// Provides extension methods to the <see cref="IEnumerable{T}"/> and similar interfaces.
    /// </summary>
    internal static class EnumerableExtensions
    {
        /// <summary>
        /// Converts an <see cref="IEnumerable{T}"/> to an <see cref="IReadOnlyList{T}"/>, but calls <see cref="Enumerable.ToList{TSource}"/>
        /// only if needed.
        /// </summary>
        public static IReadOnlyList<T> ToReadOnlyList<T>( this IEnumerable<T> collection ) => collection is IReadOnlyList<T> list ? list : collection.ToList();

        /// <summary>
        /// Converts an <see cref="IEnumerable"/> to an <see cref="IReadOnlyList{T}"/>, but calls <see cref="Enumerable.ToList{TSource}"/>
        /// only if needed.
        /// </summary>
        public static IReadOnlyList<object> ToReadOnlyList( this IEnumerable collection )
            => collection is IReadOnlyList<object> list ? list : new List<object>( collection.Cast<object>() );

        /// <summary>
        /// Appends a set of items to a list.
        /// </summary>
        public static void AddRange<T>( this IList<T> list, IEnumerable<T> items )
        {
            foreach ( var item in items )
            {
                list.Add( item );
            }
        }

        /// <summary>
        /// Selects the closure of a graph. This is typically used to select all descendants of a tree node.  This method returns distinct nodes only.
        /// </summary>
        /// <param name="item">The initial item.</param>
        /// <param name="getItems">A function that returns the set of all nodes connected to a given node.</param>
        /// <param name="includeThis">A value indicating whether <paramref name="item"/> itself should be included in the result set.</param>
        /// <param name="throwOnDuplicate"><c>true</c> if an exception must be thrown if a duplicate if found.</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerable<T> SelectManyRecursive<T>(
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
                        throw new AssertionFailedException( $"The item {item} of type {item.GetType().Name} has been visited twice." );
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

            recursionCheck--;
        }

        /// <summary>
        /// Builds an <see cref="ImmutableMultiValueDictionary{TKey,TValue}"/> from a collection, with a different value type than the input item type.
        /// </summary>
        public static ImmutableMultiValueDictionary<TKey, TValue> ToMultiValueDictionary<TItem, TKey, TValue>(
            this IEnumerable<TItem> enumerable,
            Func<TItem, TKey> getKey,
            Func<TItem, TValue> getValue,
            IEqualityComparer<TKey>? keyComparer = null )
            where TKey : notnull
            => ImmutableMultiValueDictionary<TKey, TValue>.Create( enumerable, getKey, getValue, keyComparer );

        /// <summary>
        /// Builds an <see cref="ImmutableMultiValueDictionary{TKey,TValue}"/> from a collection.
        /// </summary>
        public static ImmutableMultiValueDictionary<TKey, TItem> ToMultiValueDictionary<TItem, TKey>(
            this IEnumerable<TItem> enumerable,
            Func<TItem, TKey> getKey,
            IEqualityComparer<TKey>? keyComparer = null )
            where TKey : notnull
            => enumerable.ToMultiValueDictionary( getKey, i => i, keyComparer );
    }
}