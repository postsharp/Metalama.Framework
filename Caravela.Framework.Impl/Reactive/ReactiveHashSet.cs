using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Caravela.Reactive
{
    public class ReactiveHashSet<T> : ICollection<T>, IReactiveCollection<T>, IReadOnlyCollection<T>
    {
        private readonly ObserverList<IReactiveCollectionObserver<T>> _observers;
        private ImmutableHashSet<T> _items = ImmutableHashSet<T>.Empty;
        private int _version;

        public ReactiveHashSet()
        {
            _observers = new ObserverList<IReactiveCollectionObserver<T>>(this);
        }

        protected object WriteSync => _observers;

        private ReactiveVersionedValue<IEnumerable<T>> VersionedValue =>
            new ReactiveVersionedValue<IEnumerable<T>>(this, _version);


        public IEnumerator<T> GetEnumerator()
        {
            return _items.GetEnumerator();
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        void ICollection<T>.Add(T item)
        {
            Add(item);
        }

        public void Clear()
        {
            if (!_items.IsEmpty)
                lock (WriteSync)
                {
                    IncrementVersion();

                    var oldItems = _items;

                    _items = ImmutableHashSet<T>.Empty;

                    if (!_observers.IsEmpty)
                        foreach (var subscription in _observers)
                            subscription.Observer.OnValueChanged(subscription, oldItems, _items, _version, true);
                }
        }

        public bool Contains(T item)
        {
            return _items.Contains(item);
        }


        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(T item)
        {
            if (!_items.Contains(item))
                return false;

            lock (WriteSync)
            {
                var items = _items;

                items = items.Remove(item);

                if (items != _items)
                {
                    var oldItems = _items;
                    _items = items;

                    IncrementVersion();

                    if (!_observers.IsEmpty)
                        foreach (var subscription in _observers)
                        {
                            subscription.Observer.OnItemRemoved(subscription, item, _version);
                            subscription.Observer.OnValueChanged(subscription, oldItems, items, _version);
                        }

                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public int Count => _items.Count;

        public bool IsReadOnly => false;


        IReactiveSubscription IReactiveObservable<IReactiveCollectionObserver<T>>.AddObserver(
            IReactiveCollectionObserver<T> observer)
        {
            return _observers.AddObserver(observer);
        }

        bool IReactiveObservable<IReactiveCollectionObserver<T>>.RemoveObserver(IReactiveSubscription subscription)
        {
            return _observers.RemoveObserver(subscription);
        }

        // TODO: this is not thread-safe.
        IEnumerable<T> IReactiveSource<IEnumerable<T>, IReactiveCollectionObserver<T>>.GetValue(
            in ReactiveCollectorToken collectorToken)
        {
            return this;
        }

        IReactiveVersionedValue<IEnumerable<T>> IReactiveSource<IEnumerable<T>, IReactiveCollectionObserver<T>>.
            GetVersionedValue(in ReactiveCollectorToken collectorToken)
        {
            return VersionedValue;
        }

        public bool IsMaterialized => true;


        protected void IncrementVersion()
        {
            _version++;
        }

        public bool Add(T item)
        {
            if (Contains(item)) return false;


            lock (WriteSync)
            {
                var items = _items;

                items = items.Add(item);

                if (items != _items)
                {
                    var oldItems = _items;
                    _items = items;

                    IncrementVersion();

                    if (!_observers.IsEmpty)
                        foreach (var subscription in _observers)
                        {
                            subscription.Observer.OnItemAdded(subscription, item, _version);
                            subscription.Observer.OnValueChanged(subscription, oldItems, items, _version);
                        }

                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool Replace(T oldItem, T newItem)
        {
            if (!_items.Contains(oldItem)) return false;

            lock (WriteSync)
            {
                var items = _items;

                items = items.Remove(oldItem);
                items = items.Add(newItem);

                if (items != _items)
                {
                    var oldItems = _items;
                    _items = items;

                    IncrementVersion();

                    if (!_observers.IsEmpty)
                        foreach (var subscription in _observers)
                        {
                            subscription.Observer.OnItemAdded(subscription, newItem, _version);
                            subscription.Observer.OnItemReplaced(subscription, oldItem, newItem, _version);
                            subscription.Observer.OnValueChanged(subscription, oldItems, items, _version);
                        }
                }
            }

            return true;
        }
    }
}