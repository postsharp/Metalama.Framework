// unset

using System.Collections.Generic;
using System.Linq;

namespace Caravela.Framework.Collections
{
    public interface IReadOnlyMultiValueDictionary<TKey, out TValue> : IEnumerable<IGrouping<TKey, TValue>>
    {
        IReadOnlyList<TValue> this[TKey key] { get; }
        IEnumerable<TKey> Keys { get; }
    }
}