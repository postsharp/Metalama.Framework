﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.Advising
{
    internal sealed class ObjectReaderMergeWrapper : IObjectReader
    {
        private readonly ImmutableDictionary<string, object?> _inner;

        public ObjectReaderMergeWrapper( IObjectReader?[] readers )
        {
            var builder = ImmutableDictionary<string, object?>.Empty.ToBuilder();

            foreach ( var reader in readers )
            {
                if ( reader == null )
                {
                    continue;
                }

                foreach ( var kvp in reader )
                {
                    builder[kvp.Key] = kvp.Value;
                }
            }

            this._inner = builder.ToImmutable();
        }

        public object? this[ string key ] => this._inner[key];

        public object Source => this._inner;

        public IEnumerable<string> Keys => this._inner.Keys;

        public IEnumerable<object?> Values => this._inner.Values;

        public int Count => this._inner.Count;

        public bool ContainsKey( string key ) => this._inner.ContainsKey( key );

        public IEnumerator<KeyValuePair<string, object?>> GetEnumerator() => this._inner.GetEnumerator();

        public bool TryGetValue( string key, out object? value ) => this._inner.TryGetValue( key, out value );

        IEnumerator IEnumerable.GetEnumerator() => this._inner.GetEnumerator();
    }
}