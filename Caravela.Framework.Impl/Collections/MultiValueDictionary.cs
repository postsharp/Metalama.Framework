// unset

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Caravela.Framework.Collections;

namespace Caravela.Framework.Impl.Collections
{
    public class MultiValueDictionary<TKey, TValue> : IReadOnlyMultiValueDictionary<TKey,TValue>
    {
        private Dictionary<TKey, Group> _dictionary = new();

        public MultiValueDictionary( )
        {
        }
        
        public MultiValueDictionary( IEnumerable<KeyValuePair<TKey, TValue>> pairs )
        {
            this.AddRange( pairs );
        }

        public void AddRange( IEnumerable<KeyValuePair<TKey, TValue>> pairs )
        {
            foreach ( var pair in pairs )
            {
                this.Add( pair.Key, pair.Value );
            }
        }

        public void Add( TKey key, TValue value )
        {
            if ( this._dictionary.TryGetValue( key, out var list ) )
            {
                list.Add( value );
            }
            else
            {
                list = new Group(key);
                list.Add( value );
                this._dictionary.Add( key, list );
            }
        }
        

        public IReadOnlyList<TValue> this[ TKey key ]
        {
            get
            {
                if ( this._dictionary.TryGetValue( key, out var list ) )
                {
                    return list;
                }
                else
                {
                    return Array.Empty<TValue>();
                }
            }
        }

        public IReadOnlyCollection<TKey> Keys => this._dictionary.Keys;

        class Group : List<TValue>, IGrouping<TKey, TValue>
        {
            public Group( TKey key )
            {
                this.Key = key;
            }

            public TKey Key { get; }
        }

        public IEnumerator<IGrouping<TKey, TValue>> GetEnumerator() => this._dictionary.Values.GetEnumerator();
        
        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }
}