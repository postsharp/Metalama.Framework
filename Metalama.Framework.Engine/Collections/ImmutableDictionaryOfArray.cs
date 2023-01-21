// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Metalama.Framework.Engine.Collections
{
    [PublicAPI]
    public sealed partial class ImmutableDictionaryOfArray<TKey, TValue> : IReadOnlyMultiValueDictionary<TKey, TValue>
        where TKey : notnull
    {
        private readonly ImmutableDictionary<TKey, Group> _dictionary;

        private ImmutableDictionaryOfArray( ImmutableDictionary<TKey, Group> dictionary )
        {
            this._dictionary = dictionary;
        }

        public static ImmutableDictionaryOfArray<TKey, TValue> Empty => new( ImmutableDictionary<TKey, Group>.Empty );

        public static ImmutableDictionaryOfArray<TKey, TValue> Create( IEqualityComparer<TKey> comparer )
            => new( ImmutableDictionary.Create<TKey, Group>( comparer ) );

        // Coverage: ignore
        public static ImmutableDictionaryOfArray<TKey, TValue> Create(
            IEnumerable<TValue> source,
            Func<TValue, TKey> getKey,
            IEqualityComparer<TKey>? comparer = null )
            => Create( source, getKey, v => v, comparer );

        public static ImmutableDictionaryOfArray<TKey, TValue> Create<TItem>(
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

        public ImmutableDictionaryOfArray<TKey, TValue> AddRange( IEnumerable<TValue> source, Func<TValue, TKey> getKey )
            => this.AddRange( source, getKey, v => v );

        public ImmutableDictionaryOfArray<TKey, TValue> AddRange<TItem>( IEnumerable<TItem> source, Func<TItem, TKey> getKey, Func<TItem, TValue> getValue )
        {
            var builder = this.ToBuilder();
            builder.AddRange( source, getKey, getValue );

            return builder.ToImmutable();
        }

        IReadOnlyCollection<TValue> IReadOnlyMultiValueDictionary<TKey, TValue>.this[ TKey key ] => this[key];

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

        public bool IsEmpty => this._dictionary.IsEmpty;

        public IEnumerator<IGrouping<TKey, TValue>> GetEnumerator() => this._dictionary.Values.Cast<IGrouping<TKey, TValue>>().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        public Builder ToBuilder() => new( this );

        public ImmutableDictionaryOfArray<TKey, TValue> WithKeyComparer( IEqualityComparer<TKey> keyComparer )
        {
            var dictionaryBuilder = ImmutableDictionary.CreateBuilder<TKey, Group>( keyComparer );

            dictionaryBuilder.AddRange( this._dictionary );

            return new ImmutableDictionaryOfArray<TKey, TValue>( dictionaryBuilder.ToImmutable() );
        }
    }
}