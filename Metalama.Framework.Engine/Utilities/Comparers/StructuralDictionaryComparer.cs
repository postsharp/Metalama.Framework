// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.Utilities;

internal class StructuralDictionaryComparer<TKey, TValue> : IEqualityComparer<IReadOnlyDictionary<TKey, TValue>>
{
    private readonly IEqualityComparer<TValue> _valueComparer;

    public StructuralDictionaryComparer( IEqualityComparer<TValue> valueComparer )
    {
        this._valueComparer = valueComparer;
    }

    public bool Equals( IReadOnlyDictionary<TKey, TValue> x, IReadOnlyDictionary<TKey, TValue> y )
    {
        if ( x.Count != y.Count )
        {
            return false;
        }

        foreach ( var xPair in x )
        {
            if ( !y.TryGetValue( xPair.Key, out var xValue ) )
            {
                return false;
            }

            if ( !this._valueComparer.Equals( xPair.Value, xValue ) )
            {
                return false;
            }
        }

        return true;
    }

    public int GetHashCode( IReadOnlyDictionary<TKey, TValue> obj )
    {
        var hashCode = default(HashCode);

        foreach ( var item in obj )
        {
            hashCode.Add( item.Key );
            hashCode.Add( item.Value );
        }

        return hashCode.ToHashCode();
    }
}