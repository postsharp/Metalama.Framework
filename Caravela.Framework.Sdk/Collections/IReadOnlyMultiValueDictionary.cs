// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Caravela.Framework.Impl.Collections
{
    public interface IReadOnlyMultiValueDictionary<TKey, TValue> : IEnumerable<IGrouping<TKey, TValue>>
        where TKey : notnull
    {
        ImmutableArray<TValue> this[ TKey key ] { get; }

        IEnumerable<TKey> Keys { get; }
    }
}