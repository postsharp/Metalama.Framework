#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

#endregion

namespace Caravela.Reactive
{
    public class ReactiveHashSet<T> : ICollection<T>, IReactiveCollection<T>, IReadOnlyCollection<T>
    {
        private readonly ObserverList<IReactiveCollectionObserver<T>> _observers;
        private ImmutableHashSet<T> _items = ImmutableHashSet<T>.Empty;
        private int _version;

        public ReactiveHashSet()
        {
            this._observers = new ObserverList<IReactiveCollectionObserver<T>>(this);
        }

        public ReactiveHashSet(params T[] items) : this((IEnumerable<T>) items)
        {
            
        }

        public ReactiveHashSet(IEnumerable<T> items) : this()
        {
            _items = items.ToImmutableHashSet();
        }

        protected object WriteSync => this._observers;

        private ReactiveVersionedValue<IEnumerable<T>> VersionedValue =>
            new ReactiveVersionedValue<IEnumerable<T>>(this, this._version);


        public IEnumerator<T> GetEnumerator()
        {
            return this._items.GetEnumerator();
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }


        void ICollection<T>.Add(T item)
        {
            this.Add(item);
        }

        public void Clear()
        {
            if (!this._items.IsEmpty)
            {
                lock (this.WriteSync)
                {
                    this.IncrementVersion();

                    var oldItems = this._items;

                    this._items = ImmutableHashSet<T>.Empty;

                    foreach (var subscription in this._observers)
                    {
                        subscription.Observer.OnValueChanged(subscription.Subscription, oldItems, this._items, this._version,
                            true);
                    }
                }
            }
        }

        public bool Contains(T item)
        {
            return this._items.Contains(item);
        }


        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(T item)
        {
            if (!this._items.Contains(item))
            {
                return false;
            }

            lock (this.WriteSync)
            {
                var items = this._items;

                items = items.Remove(item);

                if (items != this._items)
                {
                    var oldItems = this._items;
                    this._items = items;

                    this.IncrementVersion();

                    foreach (var subscription in this._observers)
                    {
                        subscription.Observer.OnItemRemoved(subscription.Subscription, item, this._version);
                    }
                    
                    foreach (var subscription in this._observers.OfType<IEnumerable<T>>())
                    {
                        subscription.Observer.OnValueChanged(subscription.Subscription, oldItems, items, this._version);
                    }

                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public int Count => this._items.Count;

        public bool IsReadOnly => false;


        object IReactiveObservable<IReactiveCollectionObserver<T>>.Object => this;

        IReactiveSubscription IReactiveObservable<IReactiveCollectionObserver<T>>.AddObserver(
            IReactiveCollectionObserver<T> observer)
        {
            return this._observers.AddObserver(observer);
        }

        bool IReactiveObservable<IReactiveCollectionObserver<T>>.RemoveObserver(IReactiveSubscription subscription)
        {
            return this._observers.RemoveObserver(subscription);
        }

        public bool HasPathToObserver(object observer)
        {
            return this._observers.HasPathToObserver(observer);
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
            return this.VersionedValue;
        }

        public bool IsMaterialized => true;


        protected void IncrementVersion()
        {
            this._version++;
        }

        public bool Add(T item)
        {
            if (this.Contains(item))
            {
                return false;
            }


            lock (this.WriteSync)
            {
                var items = this._items;

                items = items.Add(item);

                if (items != this._items)
                {
                    var oldItems = this._items;
                    this._items = items;

                    this.IncrementVersion();

                    foreach (var subscription in this._observers)
                    {
                        subscription.Observer.OnItemAdded(subscription.Subscription, item, this._version);
                    }
                    
                    foreach (var subscription in this._observers.OfType<IEnumerable<T>>())
                    {
                        subscription.Observer.OnValueChanged(subscription.Subscription, oldItems, items, this._version);
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
            if (!this._items.Contains(oldItem))
            {
                return false;
            }

            lock (this.WriteSync)
            {
                var items = this._items;

                items = items.Remove(oldItem);
                items = items.Add(newItem);

                if (items != this._items)
                {
                    var oldItems = this._items;
                    this._items = items;

                    this.IncrementVersion();

                    foreach (var subscription in this._observers)
                    {
                        subscription.Observer.OnItemReplaced(subscription.Subscription, oldItem, newItem, this._version);
                    }
                    
                    foreach (var subscription in this._observers.OfType<IEnumerable<T>>())
                    {
                        subscription.Observer.OnValueChanged(subscription.Subscription, oldItems, items, this._version);
                    }
                }
            }

            return true;
        }
    }
}