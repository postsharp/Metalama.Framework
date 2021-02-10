using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Caravela.Framework.Collections;
using System.Collections.Immutable;

namespace Caravela.Framework.Impl.Collections
{
    public class ImmutableMultiValueDictionary<TKey, TValue> : IReadOnlyMultiValueDictionary<TKey,TValue>
    {
        private readonly ImmutableDictionary<TKey, Group> _dictionary;


        private ImmutableMultiValueDictionary( ImmutableDictionary<TKey, Group> dictionary )
        {
            this._dictionary = dictionary;
        }

        public static ImmutableMultiValueDictionary<TKey, TValue> Empty => new( ImmutableDictionary<TKey, Group>.Empty );

        public static ImmutableMultiValueDictionary<TKey, TValue> Create<TItem>( IEnumerable<TItem> source, Func<TItem,TKey> getKey, Func<TItem,TValue> getValue )
        {
            var builder = ImmutableDictionary.CreateBuilder<TKey, Group>();

            return AddRange(builder, source, getKey, getValue);
        }

        private static ImmutableMultiValueDictionary<TKey, TValue> AddRange<TItem>(ImmutableDictionary<TKey, Group>.Builder builder, IEnumerable<TItem> source, Func<TItem, TKey> getKey, Func<TItem, TValue> getValue)
        {
            foreach (var item in source)
            {
                var key = getKey(item);
                var value = getValue(item);

                if (!builder.TryGetValue(key, out var group))
                {
                    @group = new Group(key, ImmutableArray<TValue>.Empty);
                }

                @group = @group.Add(value);

                builder[key] = @group;
            }

            return new ImmutableMultiValueDictionary<TKey, TValue>(builder.ToImmutable());
        }

        public ImmutableMultiValueDictionary<TKey, TValue> AddRange<TItem>( IEnumerable<TItem> source, Func<TItem, TKey> getKey, Func<TItem, TValue> getValue )
        {
            var builder = this._dictionary.ToBuilder();

            return AddRange( builder, source, getKey, getValue );
        }

        IReadOnlyList<TValue> IReadOnlyMultiValueDictionary<TKey, TValue>.this[ TKey key ] => this[key];

        public ImmutableArray<TValue> this[ TKey key ]
        {
            get
            {
                if ( this._dictionary.TryGetValue( key, out var group ) )
                {
                    return group.Items;
                }
                else
                {
                    return ImmutableArray<TValue>.Empty;
                }
            }
        }

        public IEnumerable<TKey> Keys => this._dictionary.Keys;

        readonly struct Group : IGrouping<TKey, TValue>
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

        public IEnumerator<IGrouping<TKey, TValue>> GetEnumerator()
            => this._dictionary.Values.Cast<IGrouping<TKey, TValue>>().GetEnumerator();
        

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

    }
}