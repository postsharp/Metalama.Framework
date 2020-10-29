using System.Collections.Generic;

namespace Caravela.Reactive.Sources
{
    /// <summary>
    /// Collection that implements the reactive interface, but does not actually ever change.
    /// </summary>
    internal sealed class ImmutableReactiveCollection<T> : IReactiveCollection<T>, IReactiveObservable<IReactiveCollectionObserver<T>>
    {
        private readonly ReactiveVersionedValue<IEnumerable<T>> _value;

        public ImmutableReactiveCollection( IEnumerable<T> items )
        {
            this._value = new ReactiveVersionedValue<IEnumerable<T>>( items, 0 );
        }

        IReactiveSource IReactiveObservable<IReactiveCollectionObserver<T>>.Source => this;

        bool IReactiveSource.IsMaterialized => true;

        bool IReactiveSource.IsImmutable => true;

        int IReactiveObservable<IReactiveCollectionObserver<T>>.Version => 0;

        IReactiveObservable<IReactiveCollectionObserver<T>> IReactiveSource<IEnumerable<T>, IReactiveCollectionObserver<T>>.Observable => this;

        IReactiveSubscription? IReactiveObservable<IReactiveCollectionObserver<T>>.AddObserver( IReactiveCollectionObserver<T> observer ) => null;

        bool IReactiveObservable<IReactiveCollectionObserver<T>>.RemoveObserver( IReactiveSubscription subscription ) { return true; }

        IEnumerable<T> IReactiveSource<IEnumerable<T>>.GetValue( in ReactiveCollectorToken observerToken ) => this._value.Value;

        IReactiveVersionedValue<IEnumerable<T>> IReactiveSource<IEnumerable<T>>.GetVersionedValue( in ReactiveCollectorToken observerToken )
            => this._value;
    }
}
