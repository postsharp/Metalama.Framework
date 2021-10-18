// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Caravela.Framework.Impl.Collections
{
    internal partial class ImmutableMultiValueDictionary<TKey, TValue>
    {
        public class Builder
        {
            private readonly ImmutableMultiValueDictionary<TKey, TValue>? _initialValues;

            private readonly ImmutableDictionary<TKey, ImmutableArray<TValue>.Builder>.Builder _newValuesBuilder;

            // Creates a Builder initialized to an empty dictionary.
            internal Builder( IEqualityComparer<TKey>? comparer )
            {
                this._newValuesBuilder = ImmutableDictionary.CreateBuilder<TKey, ImmutableArray<TValue>.Builder>( comparer );
            }

            internal Builder( ImmutableMultiValueDictionary<TKey, TValue> initialValues )
            {
                this._initialValues = initialValues;
                this._newValuesBuilder = ImmutableDictionary.CreateBuilder<TKey, ImmutableArray<TValue>.Builder>( initialValues._dictionary.KeyComparer );
            }

            public void Add( TKey key, TValue value )
            {
                if ( !this._newValuesBuilder.TryGetValue( key, out var arrayBuilder ) )
                {
                    arrayBuilder = ImmutableArray.CreateBuilder<TValue>();
                    this._newValuesBuilder[key] = arrayBuilder;
                }

                arrayBuilder.Add( value );
            }

            public void AddRange<TItem>( IEnumerable<TItem> source, Func<TItem, TKey> getKey, Func<TItem, TValue> getValue )
            {
                foreach ( var item in source )
                {
                    var key = getKey( item );
                    var value = getValue( item );

                    this.Add( key, value );
                }
            }

            public void AddRange<TItem>( IEnumerable<TItem> source, Func<TItem, TKey> getKey, Func<TItem, IEnumerable<TValue>> getValues )
            {
                foreach ( var item in source )
                {
                    var key = getKey( item );
                    var values = getValues( item );

                    this.AddRange( key, values );
                }
            }

            public void AddRange( TKey key, IEnumerable<TValue> values )
            {
                if ( !this._newValuesBuilder.TryGetValue( key, out var arrayBuilder ) )
                {
                    arrayBuilder = ImmutableArray.CreateBuilder<TValue>();
                    this._newValuesBuilder[key] = arrayBuilder;
                }

                arrayBuilder.AddRange( values );
            }

            public ImmutableMultiValueDictionary<TKey, TValue> ToImmutable()
            {
                if ( this._newValuesBuilder.Count == 0 )
                {
                    return this._initialValues ?? Empty;
                }
                else
                {
                    var dictionaryBuilder = this._initialValues?._dictionary.ToBuilder()
                                            ?? ImmutableDictionary.CreateBuilder<TKey, Group>( this._newValuesBuilder.KeyComparer );

                    foreach ( var newGroup in this._newValuesBuilder )
                    {
                        if ( !dictionaryBuilder.TryGetValue( newGroup.Key, out var group ) )
                        {
                            // This is a new group.

                            group = new Group( newGroup.Key, newGroup.Value.ToImmutable() );
                        }
                        else
                        {
                            // Existing group. Need to merge.
                            group = new Group( group.Key, group.Items.AddRange( newGroup.Value.ToImmutable() ) );
                        }

                        dictionaryBuilder[group.Key] = group;
                    }

                    return new ImmutableMultiValueDictionary<TKey, TValue>( dictionaryBuilder.ToImmutable() );
                }
            }
        }
    }
}