using System.Collections.Generic;

namespace Caravela.Reactive
{
    public interface IReactiveCollection<T> : IReactiveSource<IEnumerable<T>, IReactiveCollectionObserver<T>>
    {
    }
}