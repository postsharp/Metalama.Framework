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

        bool IReactiveSource<IEnumerable<T>, IReactiveCollectionObserver<T>>.IsMaterialized => true;

        IReactiveSubscription IReactiveObservable<IReactiveCollectionObserver<T>>.AddObserver(IReactiveCollectionObserver<T> observer) => _observers.AddObserver(observer);
        bool IReactiveObservable<IReactiveCollectionObserver<T>>.RemoveObserver(IReactiveSubscription subscription) => _observers.RemoveObserver(subscription);

        IEnumerable<T> IReactiveSource<IEnumerable<T>, IReactiveCollectionObserver<T>>.GetValue(in ReactiveCollectorToken collectorToken) => _items;

        IReactiveVersionedValue<IEnumerable<T>> IReactiveSource<IEnumerable<T>, IReactiveCollectionObserver<T>>.GetVersionedValue(in ReactiveCollectorToken collectorToken)
            => new ReactiveVersionedValue<IEnumerable<T>>(_items, 0);
    }

    internal static class ImmutableReactiveExtensions
    {
        public static IReactiveCollection<T> ToImmutableReactive<T>(this IEnumerable<T> source) => new ImmutableReactiveCollection<T>(source);
    }
}
