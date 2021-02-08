namespace Caravela.Reactive
{
    /// <summary>
    /// The weakly typed base of <see cref="IReactiveSource{T}"/>.
    /// </summary>
    public interface IReactiveSource
    {

        /// <summary>
        /// Gets a value indicating whether the current source can be enumerated without evaluating its own sources,
        /// i.e. whether it is "cached".
        /// </summary>
        bool IsMaterialized { get; }

        /// <summary>
        /// Gets a value indicating whether the source is immutable. If <c>false</c>, adding observers will have no effect.
        /// </summary>
        bool IsImmutable { get; }
    }

    /// <summary>
    /// Defines the semantic of a reactive object. Its value can be accessed only by passing a
    /// a <see cref="ReactiveCollectorToken"/>. A dependency to the reactive object is added to the observer
    /// upon evaluating the value. However, the <see cref="ReactiveCollectorToken"/> mechanism only allows
    /// for weakly-typed observers, which excludes the representation of incremental changes.
    /// The <see cref="IReactiveSource{TValue,TObserver}"/> interface adds support for strongly-typed
    /// observers and incremental changes.
    /// </summary>
    /// <typeparam name="T">Type of the value.</typeparam>
    public interface IReactiveSource<out T> : IReactiveSource
    {

        /// <summary>
        /// Gets the value of the source. Use this method instead of <see cref="GetVersionedValue"/> when
        /// you need the value but not the version or the side values.
        /// </summary>
        /// <param name="observerToken">A token to register additional dependencies and side values.</param>
        /// <returns></returns>
        T GetValue( in ReactiveCollectorToken observerToken = default );

        /// <summary>
        /// Gets the value of the source, the value version, and the side values.
        /// </summary>
        /// <param name="observerToken">A token to register additional dependencies and side values.</param>
        /// <returns></returns>
        IReactiveVersionedValue<T> GetVersionedValue( in ReactiveCollectorToken observerToken = default );
    }

    /// <summary>
    /// Defines the semantics of a reactive object that can be observed by strongly-typed observers.
    /// </summary>
    /// <typeparam name="TValue">Type of the value.</typeparam>
    /// <typeparam name="TObserver">Type of the observer.</typeparam>
    public interface IReactiveSource<out TValue, in TObserver> :
        IReactiveSource<TValue>
        where TObserver : IReactiveObserver<TValue>
    {
        /// <summary>
        /// Gets the implementation of the <see cref="IReactiveObservable{T}"/> interface. It may, but may not,
        /// be the same object as the source itself.
        /// </summary>
        IReactiveObservable<TObserver> Observable { get; }
    }
}