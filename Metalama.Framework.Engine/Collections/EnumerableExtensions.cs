// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.Collections
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

        public static HashSet<T> ToHashSet<T>( this IEnumerable<T> collection, IEqualityComparer<T>? comparer = null )
        {
            var hashSet = new HashSet<T>( comparer );

            foreach ( var item in collection )
            {
                hashSet.Add( item );
            }

            return hashSet;
        }

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
        /// Builds an <see cref="ImmutableDictionaryOfArray{TKey,TValue}"/> from a collection, with a different value type than the input item type.
        /// </summary>
        internal static ImmutableDictionaryOfArray<TKey, TValue> ToMultiValueDictionary<TItem, TKey, TValue>(
            this IEnumerable<TItem> enumerable,
            Func<TItem, TKey> getKey,
            Func<TItem, TValue> getValue,
            IEqualityComparer<TKey>? keyComparer = null )
            where TKey : notnull
            => ImmutableDictionaryOfArray<TKey, TValue>.Create( enumerable, getKey, getValue, keyComparer );

        /// <summary>
        /// Builds an <see cref="ImmutableDictionaryOfArray{TKey,TValue}"/> from a collection, with the same value type than the input item type.
        /// </summary>
        /// <summary>
        /// Builds an <see cref="ImmutableDictionaryOfArray{TKey,TValue}"/> from a collection.
        /// </summary>
        internal static ImmutableDictionaryOfArray<TKey, TItem> ToMultiValueDictionary<TItem, TKey>(
            this IEnumerable<TItem> enumerable,
            Func<TItem, TKey> getKey,
            IEqualityComparer<TKey>? keyComparer = null )
            where TKey : notnull
            => enumerable.ToMultiValueDictionary( getKey, i => i, keyComparer );

        public static IReadOnlyCollection<T> Concat<T>( params IReadOnlyCollection<T>[] collections )
        {
            var size = 0;

            foreach ( var collection in collections )
            {
                size += collection.Count;
            }

            var list = new List<T>( size );

            foreach ( var collection in collections )
            {
                list.AddRange( collection );
            }

            return list;
        }

        public static IReadOnlyCollection<T> ConcatNotNull<T>( this IReadOnlyCollection<T> a, T? b )
            where T : class
        {
            if ( b == null )
            {
                return a;
            }

            if ( a.Count == 0 )
            {
                // ReSharper disable once RedundantExplicitArrayCreation
                return new T[] { b };
            }

            var l = new List<T>( a.Count + 1 );
            l.AddRange( a );
            l.Add( b );

            return l;
        }

        public static IReadOnlyList<T> Concat<T>( this IReadOnlyList<T> a, T b )
        {
            if ( a.Count == 0 )
            {
                return new[] { b };
            }

            var l = new List<T>( a.Count + 1 );
            l.AddRange( a );
            l.Add( b );

            return l;
        }

        public static IReadOnlyList<T> Concat<T>( this IReadOnlyList<T> a, IReadOnlyList<T>? b )
        {
            if ( b == null || b.Count == 0 )
            {
                return a;
            }

            if ( a.Count == 0 )
            {
                return b;
            }

            var l = new List<T>( a.Count + b.Count );
            l.AddRange( a );
            l.AddRange( b );

            return l;
        }
    }
}