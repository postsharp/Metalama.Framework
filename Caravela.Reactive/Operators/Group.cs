#region

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Caravela.Reactive.Implementation;

#endregion

namespace Caravela.Reactive.Operators
{
    internal class Group<TKey, TItem> : IReactiveGroup<TKey,TItem>, IReactiveObservable<IReactiveCollectionObserver<TItem>>
    {
        private static readonly IEqualityComparer<TItem> _equalityComparer =
            EqualityComparerFactory.GetEqualityComparer<TItem>();

        private ObserverList<IReactiveCollectionObserver<TItem>> _observers;
        private readonly IGroupByOperator<TKey, TItem> _parent;

        private ImmutableDictionary<TItem, int> _items;
        private int _version;

        internal Group(IGroupByOperator<TKey, TItem> parent, TKey key)
        {
            this.Key = key;
            this._observers = new ObserverList<IReactiveCollectionObserver<TItem>>(this);
            this._items = ImmutableDictionary<TItem, int>.Empty.WithComparers(_equalityComparer);
            this._parent = parent;
        }

        internal Group(IGroupByOperator<TKey, TItem> parent, IGrouping<TKey, TItem> initialContent, int mark = 0) :
            this(parent, initialContent.Key)
        {
        
            this._parent = parent;
            
            this.SetItems(initialContent);

            this.Mark = mark;
        }

        private void SetItems(IEnumerable<TItem> items)
        {
            var builder = ImmutableDictionary.CreateBuilder<TItem, int>(_equalityComparer);
            foreach (var item in items)
            {
                builder.TryGetValue(item, out int count);
                builder[item] = count + 1;
            }

            this._items = builder.ToImmutable();
        }

        public bool HasObserver => !this._observers.IsEmpty;
        
        public int Count => this._items.Count;

        public TKey Key { get; }


        public IEnumerable<TItem> GetValue(in ReactiveCollectorToken observerToken)
        {
            return this._items.Keys;
        }

        IReactiveVersionedValue<IEnumerable<TItem>> IReactiveSource<IEnumerable<TItem>>.GetVersionedValue(
                in ReactiveCollectorToken observerToken)
        {
            return new ReactiveVersionedValue<IEnumerable<TItem>>( this._items.Keys, this._version );
        }

        bool IReactiveSource.IsMaterialized => true;

        bool IReactiveSource.IsImmutable => this._parent.IsImmutable;
            
        int IReactiveObservable<IReactiveCollectionObserver<TItem>>.Version =>this._version;

        IReactiveSource IReactiveObservable<IReactiveCollectionObserver<TItem>>.Source => this;

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


        internal bool Add(TItem item)
        {
            var oldItems = this._items;

            this._items.TryGetValue(item, out int count );
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
            bool hasChange = false;
            foreach (var item in items)
            {
                hasChange |= this.Add(item);
            }

            return hasChange;
        }

        internal bool Remove(TItem item)
        {
            var oldItems = this._items;

            this._items.TryGetValue(item, out int count );

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

     

        internal void Replace(IEnumerable<TItem> items, int mark)
        {
            var oldItems = this._items;
            
            // TODO: It may pay off for performance to emit incremental changes instead of a breaking change. It would require to compare the items set before and after.
            
            this.SetItems(items);
            this._version++;
            
            foreach (var subscription in this._observers.OfType<IEnumerable<TItem>>())
            {
                subscription.Observer.OnValueChanged(subscription.Subscription, oldItems.Keys, this._items.Keys, this._version, true);
            }

            this.Mark = mark;
        }
        
        internal int Mark { get; private set; }

        IReactiveObservable<IReactiveCollectionObserver<TItem>> IReactiveSource<IEnumerable<TItem>, IReactiveCollectionObserver<TItem>>.Observable => this;

        public void Clear()
        {
            this.Replace(Array.Empty<TItem>(), 0);
        }

        public override string ToString()
        {
            return $"Group Key={this.Key}";
        }

      
    }
}