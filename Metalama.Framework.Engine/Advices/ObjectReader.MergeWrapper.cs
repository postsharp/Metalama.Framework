// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.Advices
{
    internal partial class ObjectReader
    {
        internal class MergeWrapper : IObjectReader
        {
            private readonly IReadOnlyDictionary<string, object?> _inner;

            public MergeWrapper( IObjectReader?[] readers )
            {
                var builder = ImmutableDictionary<string, object?>.Empty.ToBuilder();

                for ( var i = 0; i < readers.Length; i++ )
                {
                    if ( readers[i] == null )
                    {
                        continue;
                    }

                    foreach ( var kvp in readers[i]! )
                    {
                        builder[kvp.Key] = kvp.Value;
                    }
                }

                this._inner = builder.ToImmutable();
            }

            public object? this[ string key ] => this._inner[key];

            public object? Source => this._inner;

            public IEnumerable<string> Keys => this._inner.Keys;

            public IEnumerable<object?> Values => this._inner.Values;

            public int Count => this._inner.Count;

            public bool ContainsKey( string key ) => this._inner.ContainsKey( key );

            public IEnumerator<KeyValuePair<string, object?>> GetEnumerator() => this._inner.GetEnumerator();

            public bool TryGetValue( string key, out object? value ) => this._inner.TryGetValue( key, out value );

            IEnumerator IEnumerable.GetEnumerator() => this._inner.GetEnumerator();
        }
    }
}