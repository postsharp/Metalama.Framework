// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Caravela.Framework.Impl.Collections
{
    public static class EnumerableExtensions
    {
        public static IReadOnlyList<T> ToReadOnlyList<T>( this IEnumerable<T> collection ) => collection is IReadOnlyList<T> list ? list : collection.ToList();

        public static IReadOnlyList<object> ToReadOnlyList( this IEnumerable collection )
            => collection is IReadOnlyList<object> list ? list : new List<object>( collection.Cast<object>() );

        public static void AddRange<T>( this IList<T> list, IEnumerable<T> items )
        {
            foreach ( var item in items )
            {
                list.Add( item );
            }
        }

        public static IEnumerable<T> SelectRecursive<T>( this T item, Func<T, T?> getNext, bool includeThis = false, bool throwOnDuplicate = true )
            where T : class
        {
            HashSet<T> list = new( ReferenceEqualityComparer<T>.Instance );

            for ( var i = includeThis ? item : getNext( item ); i != null; i = getNext( i ) )
            {
                if ( !list.Add( i ) )
                {
                    // We are in a cycle.
                    if ( throwOnDuplicate )
                    {
                        throw new AssertionFailedException( $"The item {i} of type {i.GetType().Name} has been visited twice." );
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return list;
        }

        public static IEnumerable<T> SelectManyRecursive<T>(
            this T item,
            Func<T, IEnumerable<T>?> getItems,
            bool includeThis = false,
            bool throwOnDuplicate = true )
            where T : class
        {
            var recursionCheck = 0;

            HashSet<T> hashSet = new( ReferenceEqualityComparer<T>.Instance );

            if ( includeThis )
            {
                _ = hashSet.Add( item );
            }

            VisitMany( getItems( item ), getItems, hashSet, throwOnDuplicate, ref recursionCheck );

            return hashSet;
        }

        public static IEnumerable<T> SelectManyRecursive<T>(
            this IEnumerable<T> collection,
            Func<T, IEnumerable<T>?> getItems,
            bool includeFirstLevel = false,
            bool throwOnDuplicate = true )
            where T : class
        {
            var recursionCheck = 0;

            HashSet<T> hashSet = new( ReferenceEqualityComparer<T>.Instance );

            if ( includeFirstLevel )
            {
                VisitMany( collection, getItems, hashSet, throwOnDuplicate, ref recursionCheck );
            }
            else
            {
                foreach ( var item in collection )
                {
                    VisitMany( getItems( item ), getItems, hashSet, throwOnDuplicate, ref recursionCheck );
                }
            }

            return hashSet;
        }

        private static void VisitMany<T>(
            IEnumerable<T>? collection,
            Func<T, IEnumerable<T>?> getItems,
            HashSet<T> hashSet,
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
                if ( !hashSet.Add( item ) )
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

                VisitMany( getItems( item ), getItems, hashSet, throwOnDuplicate, ref recursionCheck );
            }

            recursionCheck--;
        }

        public static ImmutableMultiValueDictionary<TKey, TValue> ToMultiValueDictionary<TKey, TValue>(
            this IEnumerable<KeyValuePair<TKey, TValue>> enumerable )
            where TKey : notnull
            => ImmutableMultiValueDictionary<TKey, TValue>.Create( enumerable, p => p.Key, p => p.Value );

        public static ImmutableMultiValueDictionary<TKey, TValue> ToMultiValueDictionary<TItem, TKey, TValue>(
            this IEnumerable<TItem> enumerable,
            Func<TItem, TKey> getKey,
            Func<TItem, TValue> getValue,
            IEqualityComparer<TKey>? keyComparer = null )
            where TKey : notnull
            => ImmutableMultiValueDictionary<TKey, TValue>.Create( enumerable, getKey, getValue, keyComparer );

        public static ImmutableMultiValueDictionary<TKey, TItem> ToMultiValueDictionary<TItem, TKey>(
            this IEnumerable<TItem> enumerable,
            Func<TItem, TKey> getKey,
            IEqualityComparer<TKey>? keyComparer = null )
            where TKey : notnull
            => enumerable.ToMultiValueDictionary( getKey, i => i, keyComparer );
    }
}