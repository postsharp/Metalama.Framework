using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Caravela.Framework.Impl.Collections
{
    public interface IReadOnlyMultiValueDictionary<TKey, TValue> : IEnumerable<IGrouping<TKey,TValue>>
        where TKey : notnull
    {
        ImmutableArray<TValue> this[ TKey key ] { get; }

        IEnumerable<TKey> Keys { get; }
    }
}