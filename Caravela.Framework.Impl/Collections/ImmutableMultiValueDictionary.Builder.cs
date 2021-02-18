using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Caravela.Framework.Impl.Collections
{
    public partial class ImmutableMultiValueDictionary<TKey, TValue>
    {
        public struct Builder
        {
            private readonly ImmutableDictionary<TKey, Group>.Builder _dictionaryBuilder;

            internal Builder( ImmutableDictionary<TKey, Group>.Builder dictionaryBuilder )
            {
                this._dictionaryBuilder = dictionaryBuilder;
            }

            public void AddRange<TItem>( IEnumerable<TItem> source, Func<TItem, TKey> getKey, Func<TItem, TValue> getValue )
            {
                foreach ( var item in source )
                {
                    var key = getKey( item );
                    var value = getValue( item );

                    if ( !this._dictionaryBuilder.TryGetValue( key, out var group ) )
                    {
                        @group = new Group( key, ImmutableArray<TValue>.Empty );
                    }

                    @group = @group.Add( value );

                    this._dictionaryBuilder[key] = @group;
                }
            }

            public ImmutableMultiValueDictionary<TKey, TValue> ToImmutable()
            {
                return new ImmutableMultiValueDictionary<TKey, TValue>( this._dictionaryBuilder.ToImmutable() );
            }
        }
    }
}