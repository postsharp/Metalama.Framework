// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.Advices
{
    /// <summary>
    /// Wraps an anonymous type into a dictionary-like <see cref="ITagReader"/>.
    /// </summary>
    internal partial class TagReader : ITagReader
    {
        private static readonly ConcurrentDictionary<Type, TypeAdapter> _types = new();

        public static readonly ITagReader Empty = new EmptyReader();

        public static ITagReader GetReader( object? instance )
            => instance switch
            {
                null => Empty,
                _ => new TagReader( instance )
            };

        private readonly TypeAdapter _typeAdapter;

        private TagReader( object instance )
        {
            this._typeAdapter = _types.GetOrAdd( instance.GetType(), t => new TypeAdapter( t ) );
            this.Source = instance;
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