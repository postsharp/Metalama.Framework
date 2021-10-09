// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Caravela.Framework.Impl.Collections
{
    internal partial class ImmutableMultiValueDictionary<TKey, TValue> : IReadOnlyMultiValueDictionary<TKey, TValue>
        where TKey : notnull
    {
        private readonly ImmutableDictionary<TKey, Group> _dictionary;

        private ImmutableMultiValueDictionary( ImmutableDictionary<TKey, Group> dictionary )
        {
            this._dictionary = dictionary;
        }

        public static ImmutableMultiValueDictionary<TKey, TValue> Empty => new( ImmutableDictionary<TKey, Group>.Empty );

        // Coverage: ignore
        public static ImmutableMultiValueDictionary<TKey, TValue> Create(
            IEnumerable<TValue> source,
            Func<TValue, TKey> getKey,
            IEqualityComparer<TKey>? comparer = null )
            => Create( source, getKey, v => v, comparer );

        public static ImmutableMultiValueDictionary<TKey, TValue> Create<TItem>(
            IEnumerable<TItem> source,
            Func<TItem, TKey> getKey,
            Func<TItem, TValue> getValue,
            IEqualityComparer<TKey>? comparer = null )
        {
            var builder = new Builder( comparer );
            builder.AddRange( source, getKey, getValue );

            return builder.ToImmutable();
        }

        public static Builder CreateBuilder( IEqualityComparer<TKey>? comparer = null ) => new( comparer );

        public ImmutableMultiValueDictionary<TKey, TValue> AddRange( IEnumerable<TValue> source, Func<TValue, TKey> getKey )
            => this.AddRange( source, getKey, v => v );

        public ImmutableMultiValueDictionary<TKey, TValue> AddRange<TItem>( IEnumerable<TItem> source, Func<TItem, TKey> getKey, Func<TItem, TValue> getValue )
        {
            var builder = this.ToBuilder();
            builder.AddRange( source, getKey, getValue );

            return builder.ToImmutable();
        }

        public ImmutableArray<TValue> this[ TKey key ]
        {
            get
            {
                if ( this._dictionary.TryGetValue( key, out var group ) )
                {
                    return group.Items;
                }

                return ImmutableArray<TValue>.Empty;
            }
        }

        public IEnumerable<TKey> Keys => this._dictionary.Keys;

        public IEnumerator<IGrouping<TKey, TValue>> GetEnumerator() => this._dictionary.Values.Cast<IGrouping<TKey, TValue>>().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        public Builder ToBuilder() => new( this );

        public ImmutableMultiValueDictionary<TKey, TValue> WithKeyComparer( IEqualityComparer<TKey> keyComparer )
        {
            var dictionaryBuilder = ImmutableDictionary.CreateBuilder<TKey, Group>( keyComparer );

            dictionaryBuilder.AddRange( this._dictionary );

            return new ImmutableMultiValueDictionary<TKey, TValue>( dictionaryBuilder.ToImmutable() );
        }
    }
}