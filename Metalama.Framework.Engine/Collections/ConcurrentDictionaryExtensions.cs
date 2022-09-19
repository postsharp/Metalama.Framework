// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System.Collections.Concurrent;

namespace Metalama.Framework.Engine.Collections;

public static class ConcurrentDictionaryExtensions
{
    public static TValue GetOrAddNew<TKey, TValue>( this ConcurrentDictionary<TKey, TValue> dictionary, TKey key ) where TValue : new()
        => dictionary.GetOrAdd( key, _ => new TValue() );
}