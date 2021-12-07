// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Impl.Collections
{
    public interface IReadOnlyMultiValueDictionary<TKey, TValue> : IEnumerable<IGrouping<TKey, TValue>>
        where TKey : notnull
    {
        IReadOnlyCollection<TValue> this[ TKey key ] { get; }

        IEnumerable<TKey> Keys { get; }
    }
}