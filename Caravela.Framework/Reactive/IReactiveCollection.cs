using System.Collections.Generic;

namespace Caravela.Reactive
{
    public interface IReactiveCollection<out T> : IReactiveSource<IEnumerable<T>, IReactiveCollectionObserver<T>>
    {
    }
}