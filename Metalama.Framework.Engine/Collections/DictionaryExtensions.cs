// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

// ReSharper disable once CheckNamespace

namespace System.Collections.Generic;

public static class DictionaryExtensions
{
#if !NET6_0_OR_GREATER
    public static bool TryAdd<TKey, TValue>(
        this Dictionary<TKey, TValue> dictionary,
        TKey key,
        TValue value)
    {
        if ( dictionary.TryGetValue( key, out _) )
        {
            return false;
        }
        else
        {
            dictionary.Add( key, value );

            return true;
        }
    }

    public static TValue GetValueOrDefault<TKey, TValue>(
        this Dictionary<TKey, TValue> dictionary,
        TKey key )
    {
        if ( dictionary.TryGetValue( key, out var value ) )
        {
            return value;
        }
        else
        {
            return default;
        }
    }
#endif

    public static TValue GetOrAdd<TKey, TValue>(
        this Dictionary<TKey, TValue> dictionary,
        TKey key,
        Func<TKey, TValue> valueFactory )
    {
        if ( !dictionary.TryGetValue( key, out var value ) )
        {
            value = valueFactory( key );
            dictionary.Add( key, value );
        }

        return value;
    }
}