using System.Collections.Generic;

namespace Caravela.Reactive
{
    /// <summary>
    /// A reactive <see cref="IEnumerable{T}"/> that supports asynchronous operators.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IAsyncReactiveCollection<T> : IAsyncReactiveSource<IEnumerable<T>, IReactiveCollectionObserver<T>>
    {
    }
}