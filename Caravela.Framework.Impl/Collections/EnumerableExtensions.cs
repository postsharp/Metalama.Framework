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

        public static IEnumerable<T> SelectSelfAndAncestors<T>( this T item, Func<T, T?> getParent )
            where T : class
        {
            HashSet<T> list = new( ReferenceEqualityComparer<T>.Instance );

            for ( var i = item; i != null; i = getParent( i ) )
            {
                if ( !list.Add( i ) )
                {
                    throw new AssertionFailedException( $"The item {i} of type {i.GetType().Name} has been visited twice." );
                }
            }

            return list;
        }

        public static IEnumerable<T> SelectDescendants<T>( this IEnumerable<T> collection, Func<T, IEnumerable<T>?> getChildren )
            where T : class
        {
            var recursionCheck = 0;

            HashSet<T> list = new( ReferenceEqualityComparer<T>.Instance );

            void PopulateDescendants( T? c )
            {
                recursionCheck++;

                if ( recursionCheck > 64 )
                {
                    throw new InvalidOperationException( "Too many levels of inheritance." );
                }

                if ( c != null )
                {
                    var children = getChildren( c );

                    if ( children != null )
                    {
                        foreach ( var child in children )
                        {
                            if ( !list.Add( child ) )
                            {
                                throw new AssertionFailedException( $"The item {child} of type {child.GetType().Name} has been visited twice." );
                            }

                            PopulateDescendants( child );
                        }
                    }
                }

                recursionCheck--;
            }

            foreach ( var child in collection )
            {
                PopulateDescendants( child );
            }

            return list;
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