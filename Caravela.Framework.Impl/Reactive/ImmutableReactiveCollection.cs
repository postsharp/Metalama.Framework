using System.Collections.Generic;
using System.Collections.Immutable;
using Caravela.Reactive;

namespace Caravela.Framework.Impl.Reactive
{
    /// <summary>
    /// Collection that implements the reactive interface, but does not actually ever change.
    /// </summary>
    // TODO: every usage of this type should be probably changed to make it reactive
    internal class ImmutableReactiveCollection<T> : IReactiveCollection<T>
    {
        private readonly ImmutableArray<T> _items;
        private readonly ObserverList<IReactiveCollectionObserver<T>> _observers;

        public ImmutableReactiveCollection(IEnumerable<T> items)
        {
            _items = items.ToImmutableArray();

            _observers = new(this);
        }

        object IReactiveObservable<IReactiveCollectionObserver<T>>.Object => this;

        bool IReactiveSource<IEnumerable<T>>.IsMaterialized => true;

        bool IReactiveSource<IEnumerable<T>>.IsImmutable => true;

        int IReactiveObservable<IReactiveCollectionObserver<T>>.Version => 0;

        IReactiveSubscription IReactiveObservable<IReactiveCollectionObserver<T>>.AddObserver(IReactiveCollectionObserver<T> observer) => _observers.AddObserver(observer);
        bool IReactiveObservable<IReactiveCollectionObserver<T>>.RemoveObserver(IReactiveSubscription subscription) => _observers.RemoveObserver(subscription);

        IEnumerable<T> IReactiveSource<IEnumerable<T>>.GetValue(in ReactiveObserverToken observerToken) => _items;

        IReactiveVersionedValue<IEnumerable<T>> IReactiveSource<IEnumerable<T>>.GetVersionedValue(in ReactiveObserverToken observerToken)
            => new ReactiveVersionedValue<IEnumerable<T>>(_items, 0);
    }

    internal static class ImmutableReactiveExtensions
    {
        public static IReactiveCollection<T> ToImmutableReactive<T>(this IEnumerable<T> source) => new ImmutableReactiveCollection<T>(source);
    }
}
