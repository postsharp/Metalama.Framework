// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Generic;

namespace Caravela.Framework.Impl
{
    internal static class Extensions
    {
        public static void Deconstruct<TKey, TValue>( this KeyValuePair<TKey, TValue> keyValuePair, out TKey key, out TValue value )
        {
            key = keyValuePair.Key;
            value = keyValuePair.Value;
        }
    }
}
