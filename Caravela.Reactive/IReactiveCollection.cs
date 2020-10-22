#region

using System.Collections.Generic;

#endregion

namespace Caravela.Reactive
{
    /// <summary>
    /// A reactive <see cref="IEnumerable{T}"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IReactiveCollection<out T> : IReactiveSource<IEnumerable<T>, IReactiveCollectionObserver<T>>
    {
    }

    /// <summary>
    /// A reactive <see cref="IEnumerable{T}"/> that supports asynchronous operators.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IAsyncReactiveCollection<T> : IAsyncReactiveSource<IEnumerable<T>, IReactiveCollectionObserver<T>>
    {
    }
}