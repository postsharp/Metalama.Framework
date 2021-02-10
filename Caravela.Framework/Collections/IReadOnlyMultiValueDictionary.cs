// unset

using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;

namespace Caravela.Framework.Collections
{
    public interface IReadOnlyMultiValueDictionary<TKey, out TValue> : IEnumerable<IGrouping<TKey, TValue>>
    {
        IReadOnlyList<TValue> this[TKey key] { get; }
        IReadOnlyCollection<TKey> Keys { get; }
    }
}