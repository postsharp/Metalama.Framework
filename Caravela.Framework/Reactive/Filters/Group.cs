#region

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

#endregion

namespace Caravela.Reactive
{
    public class Group<TKey, TItem> : IReactiveCollection<TItem>, IReactiveTokenCollector
    {
        private static readonly IEqualityComparer<TItem> _equalityComparer =
            EqualityComparerFactory.GetEqualityComparer<TItem>();

        private readonly ObserverList<IReactiveCollectionObserver<TItem>> _observers;
        private readonly IEnsureSubscribedToSource _parent;

        private ImmutableDictionary<TItem, int> _items;
        private int _version;

        internal Group(IEnsureSubscribedToSource parent, TKey key)
        {
            this.Key = key;
            this._observers = new ObserverList<IReactiveCollectionObserver<TItem>>(this);
            this._items = ImmutableDictionary<TItem, int>.Empty.WithComparers(_equalityComparer);
            this._parent = parent;
        }

        internal Group(IEnsureSubscribedToSource parent, IGrouping<TKey, TItem> initialContent) : this(parent,
            initialContent.Key)
        {
            var builder = ImmutableDictionary.CreateBuilder<TItem, int>(_equalityComparer);
            foreach (var item in initialContent)
            {
                builder.TryGetValue(item, out int count);
                builder[item] = count + 1;
            }

            this._items = builder.ToImmutable();
            this._parent = parent;
        }

        public bool HasObserver => !this._observers.IsEmpty;
        
        public int Count => this._items.Count;

        public TKey Key { get; }

        private ReactiveVersionedValue<IEnumerable<TItem>> VersionedValue =>
            new ReactiveVersionedValue<IEnumerable<TItem>>(this.GetValue(new ReactiveCollectorToken(this)),
                this._version);

        public IEnumerable<TItem> GetValue(in ReactiveCollectorToken collectorToken)
        {
            return this._items.Keys;
        }

        IReactiveVersionedValue<IEnumerable<TItem>>
            IReactiveSource<IEnumerable<TItem>, IReactiveCollectionObserver<TItem>>.GetVersionedValue(
                in ReactiveCollectorToken collectorToken)
        {
            return this.VersionedValue;
        }

        bool IReactiveSource<IEnumerable<TItem>, IReactiveCollectionObserver<TItem>>.IsMaterialized => true;


        object IReactiveObservable<IReactiveCollectionObserver<TItem>>.Object => this;

        IReactiveSubscription IReactiveObservable<IReactiveCollectionObserver<TItem>>.AddObserver(
            IReactiveCollectionObserver<TItem> observer)
        {

            this._parent.EnsureSubscribedToSource();
            return this._observers.AddObserver(observer);
        }

        bool IReactiveObservable<IReactiveCollectionObserver<TItem>>.RemoveObserver(IReactiveSubscription subscription)
        {
            return this._observers.RemoveObserver(subscription);
        }

        bool IReactiveDebugging.HasPathToObserver(object observer)
        {
            return this._observers.HasPathToObserver(observer);
        }

        void IReactiveTokenCollector.AddDependency(IReactiveObservable<IReactiveObserver> observable)
        {
            if (observable != null)
            {
                throw new InvalidOperationException();
            }
        }

        internal bool Add(TItem item)
        {
            var oldItems = this._items;

            this._items.TryGetValue(item, out var count);
            this._items = this._items.SetItem(item, count + 1);

            if (count == 0)
            {
                this._version++;

                foreach (var subscription in this._observers)
                {
                    subscription.Observer.OnItemAdded(subscription.Subscription, item, this._version);
                }
                
                foreach (var subscription in this._observers.OfType<IEnumerable<TItem>>())
                {
                    subscription.Observer.OnValueChanged(subscription.Subscription, oldItems.Keys, this._items.Keys, this._version);
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
            foreach (var item in items)
            {
                hasChange |= this.Add(item);
            }

            return hasChange;
        }

        internal bool Remove(TItem item)
        {
            var oldItems = this._items;

            this._items.TryGetValue(item, out var count);

            if (count == 1)
            {
                this._items = this._items.Remove(item);
                this._version++;

                foreach (var subscription in this._observers)
                {
                    subscription.Observer.OnItemRemoved(subscription.Subscription, item, this._version);
                }
                
                foreach (var subscription in this._observers.OfType<IEnumerable<TItem>>())
                {
                    subscription.Observer.OnValueChanged(subscription.Subscription, oldItems.Keys, this._items.Keys, this._version);
                }


                return true;
            }
            else
            {
                this._items = this._items.SetItem(item, count - 1);
                return false;
            }
        }

        internal bool RemoveRange(IEnumerable<TItem> items)
        {
            var hasChange = false;
            foreach (var item in items)
            {
                hasChange |= this.Remove(item);
            }

            return hasChange;
        }
    }
}