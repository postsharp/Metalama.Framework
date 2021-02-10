// unset

using System;
using System.Collections;
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

        public static MultiValueDictionary<TKey, TValue> ToMultiValueDictionary<TKey, TValue>( this IEnumerable<KeyValuePair<TKey, TValue>> enumerable )
            => new MultiValueDictionary<TKey, TValue>( enumerable );

        public static MultiValueDictionary<TKey, TValue> ToMultiValueDictionary<TItem, TKey, TValue>( this  IEnumerable<TItem> enumerable, Func<TItem, TKey> getKey,
            Func<TItem, TValue> getValue )
        {
            MultiValueDictionary<TKey, TValue> dictionary = new();
            foreach ( var item in enumerable )
            {
                dictionary.Add( getKey(item), getValue(item) );
            }

            return dictionary;
        }


    }
}