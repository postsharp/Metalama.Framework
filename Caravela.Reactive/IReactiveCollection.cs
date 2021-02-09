using System.Collections.Generic;

namespace Caravela.Reactive
{
    /// <summary>
    /// A reactive <see cref="IEnumerable{T}"/>.
    /// </summary>
    /// <typeparam name="T">Type of items in the collection.</typeparam>
    public interface IReactiveCollection<out T> : IReactiveSource<IEnumerable<T>, IReactiveCollectionObserver<T>>
    {
    }
}