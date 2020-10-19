using System.Collections.Generic;
using System.Collections.Immutable;

namespace Caravela.Reactive
{
    /// <summary>
    /// Collection that implements the reactive interface, but does not actually ever change.
    /// </summary>
    // TODO: every usage of this type should be probably changed to make it reactive
    public sealed class ImmutableReactiveCollection<T> : IReactiveCollection<T>
    {
        private readonly ReactiveVersionedValue<IEnumerable<T>> _value;
        private readonly ObserverList<IReactiveCollectionObserver<T>> _observers;

        public ImmutableReactiveCollection(IImmutableList<T> items)
        {
            _value = new ReactiveVersionedValue<IEnumerable<T>>(items, 0);
            _observers = new(this);
        }

        public ImmutableReactiveCollection(IEnumerable<T> items) : this(items.ToImmutableList())
        {
        }

        object IReactiveObservable<IReactiveCollectionObserver<T>>.Object => this;

        bool IReactiveSource<IEnumerable<T>>.IsMaterialized => true;

        bool IReactiveSource<IEnumerable<T>>.IsImmutable => true;

        int IReactiveObservable<IReactiveCollectionObserver<T>>.Version => 0;

        IReactiveSubscription IReactiveObservable<IReactiveCollectionObserver<T>>.AddObserver(IReactiveCollectionObserver<T> observer) => _observers.AddObserver(observer);
        bool IReactiveObservable<IReactiveCollectionObserver<T>>.RemoveObserver(IReactiveSubscription subscription) => _observers.RemoveObserver(subscription);

        IEnumerable<T> IReactiveSource<IEnumerable<T>>.GetValue(in ReactiveObserverToken observerToken) => _value.Value;

        IReactiveVersionedValue<IEnumerable<T>> IReactiveSource<IEnumerable<T>>.GetVersionedValue(in ReactiveObserverToken observerToken)
            => _value;
    }

    public static class ImmutableReactiveExtensions
    {
        public static IReactiveCollection<T> ToImmutableReactive<T>(this IImmutableList<T> source) => new ImmutableReactiveCollection<T>(source);

        public static IReactiveCollection<T> ToImmutableReactive<T>(this IEnumerable<T> source) => new ImmutableReactiveCollection<T>(source);
    }
}
