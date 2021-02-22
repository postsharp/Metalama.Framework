using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Caravela.Framework.Impl.Collections
{
    public static class EnumerableExtensions
    {
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
                        var i = 0;
                        foreach ( var child in children )
                        {
                            if ( !list.Add( child ) )
                            {
                                throw new AssertionFailedException( $"The item {child} of type {child.GetType().Name} has been visited twice." );
                            }

                            PopulateDescendants( child );

                            i++;
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

        public static ImmutableMultiValueDictionary<TKey, TValue> ToMultiValueDictionary<TKey, TValue>( this IEnumerable<KeyValuePair<TKey, TValue>> enumerable )
            where TKey : notnull
            => ImmutableMultiValueDictionary<TKey, TValue>.Create( enumerable, p => p.Key, p => p.Value );

        public static ImmutableMultiValueDictionary<TKey, TValue> ToMultiValueDictionary<TItem, TKey, TValue>(
            this IEnumerable<TItem> enumerable,
            Func<TItem, TKey> getKey,
            Func<TItem, TValue> getValue,
            IEqualityComparer<TKey> keyComparer = null )
            where TKey : notnull
         => ImmutableMultiValueDictionary<TKey, TValue>.Create( enumerable, getKey, getValue, keyComparer );
    }
}