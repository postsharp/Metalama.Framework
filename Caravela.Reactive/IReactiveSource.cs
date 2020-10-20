using System.Threading;
using System.Threading.Tasks;

namespace Caravela.Reactive
{
    public interface IReactiveSource
    {

        bool IsMaterialized { get; }
        bool IsImmutable { get; }
    }

    /// <summary>
    /// Defines the semantic of a reactive object. Its value can be accessed only by passing a
    /// a <see cref="ReactiveObserverToken"/>. A dependency to the reactive object is added to the observer
    /// upon evaluating the value. However, the <see cref="ReactiveObserverToken"/> mechanism only allows
    /// for weakly-typed observers, which excludes the representation of incremental changes.
    /// The <see cref="IReactiveSource{TValue,TObserver}"/> interface adds support for strongly-typed
    /// observers and incremental changes.
    /// </summary>
    /// <typeparam name="T">Type of the value.</typeparam>
    public interface IReactiveSource<out T> : IReactiveSource
    {
        
        T GetValue(in ReactiveObserverToken observerToken = default);
        
        // Returns an interface because of covariance.
        IReactiveVersionedValue<T> GetVersionedValue(in ReactiveObserverToken observerToken = default);
    }
    
 
    /// <summary>
    /// Defines the semantics of a reactive object that can be observed by strongly-typed observers.
    /// </summary>
    /// <typeparam name="TValue">Type of the value.</typeparam>
    /// <typeparam name="TObserver">Type of the observer.</typeparam>
    public interface IReactiveSource<out TValue, in TObserver> : 
        IReactiveObservable<TObserver>,
        IReactiveSource<TValue>
        where TObserver : IReactiveObserver<TValue>
    {
     
    }

    public interface IAsyncReactiveSource<T> : IReactiveSource
    {

        ValueTask<T> GetValueAsync( ReactiveObserverToken observerToken, CancellationToken cancellationToken );

        // Returns an interface because of covariance.
        ValueTask<IReactiveVersionedValue<T>> GetVersionedValueAsync( ReactiveObserverToken observerToken, CancellationToken cancellationToken );
    }


    /// <summary>
    /// Defines the semantics of a reactive object that can be observed by strongly-typed observers.
    /// </summary>
    /// <typeparam name="TValue">Type of the value.</typeparam>
    /// <typeparam name="TObserver">Type of the observer.</typeparam>
    public interface IAsyncReactiveSource<TValue, in TObserver> :
        IReactiveObservable<TObserver>,
        IAsyncReactiveSource<TValue>
        where TObserver : IReactiveObserver<TValue>
    {

    }
}