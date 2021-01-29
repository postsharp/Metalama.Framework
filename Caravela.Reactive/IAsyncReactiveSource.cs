using System.Threading;
using System.Threading.Tasks;

namespace Caravela.Reactive
{
    /// <summary>
    /// Defines the semantics of a reactive object that can be observed by strongly-typed observers.
    /// </summary>
    /// <typeparam name="TValue">Type of the value.</typeparam>
    /// <typeparam name="TObserver">Type of the observer.</typeparam>
    public interface IAsyncReactiveSource<TValue, in TObserver> :
        IAsyncReactiveSource<TValue>
        where TObserver : IReactiveObserver<TValue>
    {
        IReactiveObservable<TObserver> Observable { get; }
    }

    public interface IAsyncReactiveSource<T> : IReactiveSource
    {
        ValueTask<T> GetValueAsync( ReactiveCollectorToken observerToken, CancellationToken cancellationToken );

        // Returns an interface because of covariance.
        ValueTask<IReactiveVersionedValue<T>> GetVersionedValueAsync( ReactiveCollectorToken observerToken,
            CancellationToken cancellationToken );
    }
}