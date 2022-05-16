// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Runtime.CompilerServices;

namespace Metalama.Framework.Engine.Utilities;

internal static class ConditionalWeakTableExtensions
{
    public static TValue GetOrAdd<TKey, TValue>( this ConditionalWeakTable<TKey, TValue> table, TKey key, Func<TKey, TValue> func ) 
        where TKey : class
        where TValue : class 
    {
        if ( table.TryGetValue( key, out var value ) )
        {
            return value;
        }
        else
        {
            lock ( table )
            {
                if ( table.TryGetValue( key, out value ) )
                {
                    return value;
                }
                else
                {
                    value = func( key );
                }
            }
        }

        return value;
    }
}