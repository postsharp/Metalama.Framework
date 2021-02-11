// unset

using System.Collections.Generic;
using System.Linq;

namespace Caravela.Framework.Collections
{
    public interface IReadOnlyMultiValueDictionary<TKey, out TValue> : IEnumerable<IGrouping<TKey, TValue>>
    {
        IReadOnlyList<TValue> GetByKey(TKey key);
        
        IEnumerable<TKey> Keys { get; }
        
        IEnumerable<TValue> Values { get; }
    }
}