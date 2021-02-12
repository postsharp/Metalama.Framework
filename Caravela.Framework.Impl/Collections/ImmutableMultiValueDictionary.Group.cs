// unset

using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Caravela.Framework.Impl.Collections
{
    public partial class ImmutableMultiValueDictionary<TKey, TValue>
    {
        private readonly struct Group : IGrouping<TKey, TValue>
        {
            public ImmutableArray<TValue> Items { get; }

            public Group( TKey key, ImmutableArray<TValue> items )
            {
                this.Key = key;
                this.Items = items;
            }

            public Group Add( TValue value ) => new( this.Key, this.Items.Add( value ) );

            public TKey Key { get; }

            public IEnumerator<TValue> GetEnumerator()
            {
                foreach ( var value in this.Items )
                {
                    yield return value;
                }
            }

            IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
        }
    }
}