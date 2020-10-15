using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Caravela.Reactive
{
    public class Group<TKey, TItem> : IReactiveCollection<TItem>, IReactiveTokenCollector
    {
        private static readonly IEqualityComparer<TItem> _equalityComparer =
            EqualityComparerFactory.GetEqualityComparer<TItem>();

        private ImmutableDictionary<TItem, int> _items;
        private readonly ObserverList<IReactiveCollectionObserver<TItem>> _observers;
        private readonly GroupByOperator<TKey, TItem> _parent;
        private int _version;

        internal Group(GroupByOperator<TKey, TItem> parent, TKey key)
        {
            Key = key;
            _observers = new ObserverList<IReactiveCollectionObserver<TItem>>(this);
            _items = ImmutableDictionary<TItem, int>.Empty.WithComparers(_equalityComparer);
            _parent = parent;
        }

        internal Group(GroupByOperator<TKey, TItem> parent, IGrouping<TKey, TItem> initialContent) : this(parent,
            initialContent.Key)
        {
            var builder = ImmutableDictionary.CreateBuilder<TItem, int>(_equalityComparer);
            foreach (var item in initialContent)
            {
                var count = 0;
                builder.TryGetValue(item, out count);
                builder[item] = count + 1;
            }

            _items = builder.ToImmutable();
            _parent = parent;
        }

        internal bool HasObserver => !_observers.IsEmpty;

        public int Count => _items.Count;

        public TKey Key { get; }

        private ReactiveVersionedValue<IEnumerable<TItem>> VersionedValue =>
            new ReactiveVersionedValue<IEnumerable<TItem>>(GetValue(new ReactiveCollectorToken(this)), _version);

        public IEnumerable<TItem> GetValue(in ReactiveCollectorToken collectorToken)
        {
            return _items.Keys;
        }

        IReactiveVersionedValue<IEnumerable<TItem>>
            IReactiveSource<IEnumerable<TItem>, IReactiveCollectionObserver<TItem>>.GetVersionedValue(
                in ReactiveCollectorToken collectorToken)
        {
            return VersionedValue;
        }

        bool IReactiveSource<IEnumerable<TItem>, IReactiveCollectionObserver<TItem>>.IsMaterialized => true;


        IReactiveSubscription IReactiveObservable<IReactiveCollectionObserver<TItem>>.AddObserver(
            IReactiveCollectionObserver<TItem> observer)
        {
            if (!HasObserver) _parent.SubscribeToSource();

            return _observers.AddObserver(observer);
        }

        bool IReactiveObservable<IReactiveCollectionObserver<TItem>>.RemoveObserver(IReactiveSubscription subscription)
        {
            return _observers.RemoveObserver(subscription);
        }

        void IReactiveTokenCollector.AddDependency(IReactiveObservable<IReactiveObserver> observable)
        {
            if (observable != null) throw new InvalidOperationException();
        }

        internal bool Add(TItem item)
        {
            var oldItems = _items;

            _items.TryGetValue(item, out var count);
            _items = _items.SetItem(item, count + 1);

            if (count == 0)
            {
                _version++;

                foreach (var subscription in _observers)
                {
                    subscription.Observer.OnItemAdded(subscription, item, _version);
                    subscription.Observer.OnValueChanged(subscription, oldItems.Keys, _items.Keys, _version);
                }

                return true;
            }
            else
            {
                return false;
            }
        }


        internal bool AddRange(IEnumerable<TItem> items)
        {
            var hasChange = false;
            foreach (var item in items) hasChange |= Add(item);

            return hasChange;
        }

        internal bool Remove(TItem item)
        {
            var oldItems = _items;

            _items.TryGetValue(item, out var count);

            if (count == 1)
            {
                _items = _items.Remove(item);
                _version++;

                foreach (var subscription in _observers)
                {
                    subscription.Observer.OnItemRemoved(subscription, item, _version);
                    subscription.Observer.OnValueChanged(subscription, oldItems.Keys, _items.Keys, _version);
                }


                return true;
            }
            else
            {
                _items = _items.SetItem(item, count - 1);
                return false;
            }
        }

        internal bool RemoveRange(IEnumerable<TItem> items)
        {
            var hasChange = false;
            foreach (var item in items) hasChange |= Remove(item);

            return hasChange;
        }
    }
}