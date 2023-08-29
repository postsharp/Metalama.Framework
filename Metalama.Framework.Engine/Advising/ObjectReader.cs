// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Metalama.Framework.Engine.Advising
{
    /// <summary>
    /// Wraps an anonymous type into a dictionary-like <see cref="IObjectReader"/>.
    /// </summary>
    internal sealed class ObjectReader : IObjectReader
    {
        private readonly ObjectReaderTypeAdapter _typeAdapter;

        public static readonly IObjectReader Empty = new ObjectReaderDictionaryWrapper( ImmutableDictionary<string, object?>.Empty );

        internal ObjectReader( object instance, ObjectReaderTypeAdapter typeAdapter )
        {
            this._typeAdapter = typeAdapter;
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

            return nonEmptyCount switch
            {
                0 => Empty,
                1 => readers[nonEmptyIndex].AssertNotNull(),
                _ => new ObjectReaderMergeWrapper( readers ),
            };
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