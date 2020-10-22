#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using Caravela.Reactive.Implementation;

#endregion

namespace Caravela.Reactive.Collections
{
    /// <summary>
    /// A reactive hash set.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class ReactiveHashSet<T> : ICollection<T>, IReactiveCollection<T>, IReadOnlyCollection<T>, IReactiveObservable<IReactiveCollectionObserver<T>>
    {
        private ObserverList<IReactiveCollectionObserver<T>> _observers;
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
            this._items = items.ToImmutableHashSet();
        }

        private object WriteSync => this._observers;

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



        IReactiveSource IReactiveObservable<IReactiveCollectionObserver<T>>.Source => this;

        IReactiveSubscription IReactiveObservable<IReactiveCollectionObserver<T>>.AddObserver(
            IReactiveCollectionObserver<T> observer)
        {
            return this._observers.AddObserver(observer);
        }

       
        bool IReactiveObservable<IReactiveCollectionObserver<T>>.RemoveObserver(IReactiveSubscription subscription)
        {
            return this._observers.RemoveObserver(subscription);
        }

        // TODO: this is not thread-safe.
        bool IReactiveSource.IsImmutable => false;

        int IReactiveObservable<IReactiveCollectionObserver<T>>.Version => this._version;

        IEnumerable<T> IReactiveSource<IEnumerable<T>>.GetValue(in ReactiveCollectorToken observerToken)
        {
            return this;
        }

        IReactiveVersionedValue<IEnumerable<T>> IReactiveSource<IEnumerable<T>>.GetVersionedValue(in ReactiveCollectorToken observerToken)
        {
            return this.VersionedValue;
        }

        public bool IsMaterialized => true;

        IReactiveObservable<IReactiveCollectionObserver<T>> IReactiveSource<IEnumerable<T>, IReactiveCollectionObserver<T>>.Observable => this;

        private void IncrementVersion()
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