// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Utilities.Caching;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Metalama.Framework.Engine.Advising
{
    /// <summary>
    /// Wraps an anonymous type into a dictionary-like <see cref="IObjectReader"/>.
    /// </summary>
    internal partial class ObjectReader : IObjectReader
    {
        private static readonly WeakCache<Type, TypeAdapter> _types = new();

        public static readonly IObjectReader Empty = new DictionaryWrapper( ImmutableDictionary<string, object?>.Empty );

        public static IObjectReader GetReader( object? instance )
            => instance switch
            {
                null => Empty,
                IObjectReader objectReader => objectReader,
                IReadOnlyDictionary<string, object?> dictionary => new DictionaryWrapper( dictionary ),
                _ => new ObjectReader( instance )
            };

        private readonly TypeAdapter _typeAdapter;

        private ObjectReader( object instance )
        {
            this._typeAdapter = _types.GetOrAdd( instance.GetType(), t => new TypeAdapter( t ) );
            this.Source = instance;
        }

        public static IObjectReader Merge( params IObjectReader?[] readers )
        {
            var nonEmptyCount = 0;
            var nonEmptyIndex = -1;

            for ( var i = 0; i < readers.Length; i++ )
            {
                if ( readers[i] != null && readers[i]!.Count > 0 )
                {
                    nonEmptyIndex = i;
                    nonEmptyCount++;

                    if ( nonEmptyCount >= 2 )
                    {
                        break;
                    }
                }
            }

            switch ( nonEmptyCount )
            {
                case 0:
                    return Empty;

                case 1:
                    return readers[nonEmptyIndex].AssertNotNull();

                default:
                    return new MergeWrapper( readers );
            }
        }

        public object? this[ string key ]
        {
            get
            {
                if ( !this.TryGetValue( key, out var value ) )
                {
                    throw new KeyNotFoundException();
                }

                return value;
            }
        }

        public IEnumerable<string> Keys => this._typeAdapter.Properties;

        public IEnumerable<object?> Values => this._typeAdapter.Properties.Select( p => this[p] );

        public bool ContainsKey( string key ) => this._typeAdapter.ContainsProperty( key );

        public bool TryGetValue( string key, out object? value ) => this._typeAdapter.TryGetValue( key, this.Source, out value );

        public IEnumerator<KeyValuePair<string, object?>> GetEnumerator()
            => this._typeAdapter.Properties.Select( p => new KeyValuePair<string, object?>( p, this[p] ) ).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        public int Count => this._typeAdapter.PropertyCount;

        public object Source { get; }
    }
}