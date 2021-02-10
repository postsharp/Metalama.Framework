// unset

using System;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.Collections
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> SelectDescendants<T>( this IEnumerable<T>? collection, Func<T, IEnumerable<T>?> getChildren )
        {
            List<T> list = new ();

            void PopulateDescendants( IEnumerable<T>? c )
            {
                if ( c != null )
                {
                    foreach ( var child in c )
                    {
                        list.Add( child );
                        PopulateDescendants( getChildren( child ) );
                    }
                }
            }

            PopulateDescendants( collection );

            return list;
        }

        public static ImmutableMultiValueDictionary<TKey, TValue> ToMultiValueDictionary<TKey, TValue>( this IEnumerable<KeyValuePair<TKey, TValue>> enumerable )
            => ImmutableMultiValueDictionary<TKey, TValue>.Create( enumerable, p => p.Key, p => p.Value );

        public static ImmutableMultiValueDictionary<TKey, TValue> ToMultiValueDictionary<TItem, TKey, TValue>( this  IEnumerable<TItem> enumerable, Func<TItem, TKey> getKey,
            Func<TItem, TValue> getValue )
         => ImmutableMultiValueDictionary<TKey, TValue>.Create( enumerable, getKey, getValue );


    }
}