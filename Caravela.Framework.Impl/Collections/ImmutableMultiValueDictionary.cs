// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Caravela.Framework.Collections;

namespace Caravela.Framework.Impl.Collections
{
    public partial class ImmutableMultiValueDictionary<TKey, TValue> : IReadOnlyMultiValueDictionary<TKey, TValue>
        where TKey : notnull
    {
        private readonly ImmutableDictionary<TKey, Group> _dictionary;

        private ImmutableMultiValueDictionary( ImmutableDictionary<TKey, Group> dictionary )
        {
            this._dictionary = dictionary;
        }

        public static ImmutableMultiValueDictionary<TKey, TValue> Empty => new( ImmutableDictionary<TKey, Group>.Empty );

        public static ImmutableMultiValueDictionary<TKey, TValue> Create( IEnumerable<TValue> source, Func<TValue, TKey> getKey, IEqualityComparer<TKey>? comparer = null )
            => Create( source, getKey, v => v, comparer );

        public static ImmutableMultiValueDictionary<TKey, TValue> Create<TItem>( IEnumerable<TItem> source, Func<TItem, TKey> getKey, Func<TItem, TValue> getValue, IEqualityComparer<TKey>? comparer = null )
        {
            var builder = new Builder( ImmutableDictionary.CreateBuilder<TKey, Group>( comparer ) );
            builder.AddRange( source, getKey, getValue );
            return builder.ToImmutable();
        }

        public static Builder CreateBuilder( IEqualityComparer<TKey>? comparer = null )
            => new Builder( ImmutableDictionary.CreateBuilder<TKey, Group>( comparer ) );

        public ImmutableMultiValueDictionary<TKey, TValue> AddRange( IEnumerable<TValue> source, Func<TValue, TKey> getKey )
            =>
                this.AddRange( source, getKey, v => v );

        public ImmutableMultiValueDictionary<TKey, TValue> AddRange<TItem>( IEnumerable<TItem> source, Func<TItem, TKey> getKey, Func<TItem, TValue> getValue )
        {
            var builder = this.ToBuilder();
            builder.AddRange( source, getKey, getValue );
            return builder.ToImmutable();
        }

        public ImmutableMultiValueDictionary<TKey, TValue> Merge( ImmutableMultiValueDictionary<TKey, TValue> other )
        {
            var builder = this.ToBuilder();
            builder.AddRange( other.SelectMany( x => x.Select( y => (x.Key, Value: y) ) ), x => x.Key, x => x.Value );
            return builder.ToImmutable();
        }

        IReadOnlyList<TValue> IReadOnlyMultiValueDictionary<TKey, TValue>.GetByKey( TKey key ) => this[key];

        public ImmutableArray<TValue> this[TKey key]
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

        public IEnumerable<TValue> Values
        {
            get
            {
                foreach ( var group in this._dictionary.Values )
                {
                    foreach ( var item in group )
                    {
                        yield return item;
                    }
                }
            }
        }

        public IEnumerator<IGrouping<TKey, TValue>> GetEnumerator()
            => this._dictionary.Values.Cast<IGrouping<TKey, TValue>>().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        public Builder ToBuilder() => new Builder( this._dictionary.ToBuilder() );

        public ImmutableMultiValueDictionary<TKey, TValue> WithKeyComparer( IEqualityComparer<TKey> keyComparer )
        {
            var innerBuilder = this._dictionary.ToBuilder();
            innerBuilder.KeyComparer = keyComparer;
            return new Builder( innerBuilder ).ToImmutable();
        }
    }
}