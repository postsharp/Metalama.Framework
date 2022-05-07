// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.Advices;

internal partial class ObjectReader
{
    private class EmptyReader : IObjectReader
    {
        public IEnumerator<KeyValuePair<string, object?>> GetEnumerator() => Enumerable.Empty<KeyValuePair<string, object?>>().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        public int Count => 0;

        public bool ContainsKey( string key ) => false;

        public bool TryGetValue( string key, out object? value )
        {
            value = null;

            return false;
        }

        public object? this[ string key ] => throw new KeyNotFoundException();

        public IEnumerable<string> Keys => Enumerable.Empty<string>();

        public IEnumerable<object?> Values => Enumerable.Empty<object?>();

        public object? Source => null;
    }
}