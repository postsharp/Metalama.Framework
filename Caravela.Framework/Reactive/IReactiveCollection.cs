#region

using System.Collections.Generic;

#endregion

namespace Caravela.Reactive
{
    public interface IReactiveCollection<out T> : IReactiveSource<IEnumerable<T>, IReactiveCollectionObserver<T>>
    {
    }
}