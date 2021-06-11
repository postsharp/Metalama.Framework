// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Generic;
using System.Linq;

namespace Caravela.Framework.Impl.Collections
{
    public interface IReadOnlyMultiValueDictionary<TKey, out TValue> : IEnumerable<IGrouping<TKey, TValue>>
    {
        IReadOnlyList<TValue> GetByKey( TKey key );

        IEnumerable<TKey> Keys { get; }

        IEnumerable<TValue> Values { get; }
    }
}