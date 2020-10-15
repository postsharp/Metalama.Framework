using System.Collections.Generic;
using Caravela.Reactive.TestModel;

namespace Caravela.Reactive
{
    public interface IReactiveCollection<T> : IReactiveSource<IEnumerable<T>, IReactiveCollectionObserver<T>>
    {
        
    }
}