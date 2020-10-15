using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace Caravela.Reactive
{
    internal class ReactiveHashSet<T> : ICollection<T>, IReactiveCollection<T>, IReactiveObserver, IReadOnlyCollection<T>
    {
        private int _version;
        private readonly ObserverList<IReactiveCollectionObserver<T>> _observers;
        private Dictionary<T, IReactiveSubscription?> _items = new();

        protected object WriteSync => _observers;
        
        public event PropertyChangedEventHandler? PropertyChanged;

        public ReactiveHashSet()
        {
            _observers = new ObserverList<IReactiveCollectionObserver<T>>(this);
        }

        private void AddCore(T item)
        {
            if (item == null)
            {
                return;
            }
            
            switch (item)
            {
                case IReactiveObservable<IReactiveObserver> observable:
                    _items[item] = observable.AddObserver(this);
                    break;
                
                case INotifyPropertyChanged notifyPropertyChanged:
                    notifyPropertyChanged.PropertyChanged += OnPropertyChanged;
                    _items[item] = null;
                    break;
                
                default:
                    _items[item] = null;
                    break;
            }
        }

        private bool RemoveCore(T item)
        {
            if (!_items.TryGetValue(item, out var subscription))
                return false;
            
            switch (item)
            {
                case IReactiveObservable<IReactiveObserver>:
                    subscription!.Dispose();
                    break;
                
                case INotifyPropertyChanged notifyPropertyChanged:
                    notifyPropertyChanged.PropertyChanged -= OnPropertyChanged;
                    break;
            }
            
            _items.Remove(item);

            return true;
        }

        private void UnfollowAll()
        {
            foreach (var pair in _items)
            {
                if (pair.Value != null)
                {
                    pair.Value.Dispose();
                }
                else
                {
                    switch (pair.Key)
                    {
                        case INotifyPropertyChanged notifyPropertyChanged:
                            notifyPropertyChanged.PropertyChanged -= OnPropertyChanged;
                            break;
                    }
                }
            }
            
            _items.Clear();
        }


        IReactiveSubscription IReactiveObservable<IReactiveCollectionObserver<T>>.AddObserver(IReactiveCollectionObserver<T> observer)
            => _observers.AddObserver(observer);

        bool IReactiveObservable<IReactiveCollectionObserver<T>>.RemoveObserver(IReactiveSubscription subscription)
            => _observers.RemoveObserver(subscription);

        public ReactiveVersionedValue<IEnumerable<T>> VersionedValue => new ReactiveVersionedValue<IEnumerable<T>>(this, _version);

        IEnumerable<T> IReactiveSource<IEnumerable<T>,IReactiveCollectionObserver<T>>.Value => this;


        protected void OnChanged()
        {
            _version++;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            foreach (var subscription in _observers)
            {
                subscription.Observer.OnItemChanged(subscription, (T)sender);
            }
        }

        public void Dispose()
        {
            if (_items != null)
            {
                UnfollowAll();
                _items = null!;
            }
        }

        void IReactiveObserver.OnReset(IReactiveSubscription subscription)
        {
            foreach (var outSubscription in _observers)
            {
                outSubscription.Observer.OnItemChanged(subscription, (T) subscription.Observer);
            }
        }

        public IEnumerator<T> GetEnumerator() => _items.Keys.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        void ICollection<T>.Add(T item) => this.Add(item);

        public bool Add(T item)
        {
            if (this.Contains(item))
            {
                return false;
            }

            lock (this.WriteSync)
            {
                AddCore(item);

                this.OnChanged();
            
                foreach (var subscription in _observers)
                {
                    subscription.Observer.OnItemAdded(subscription, item);
                }
            }

            return true;
        }

        public bool Replace(T oldItem, T newItem)
        {
            if (!_items.ContainsKey(oldItem))
            {
                return false;
            }

            lock (this.WriteSync)
            {
                RemoveCore(oldItem);
                AddCore(newItem);

                this.OnChanged();

                foreach (var subscription in _observers)
                {
                    subscription.Observer.OnItemReplaced(subscription, oldItem, newItem);
                }
            }

            return true;
        }

        public void Clear() => throw new NotImplementedException();

        public bool Contains(T item) => _items.ContainsKey(item);

        public void CopyTo(T[] array, int arrayIndex) => throw new NotImplementedException();

        public bool Remove(T item)
        {
            lock (this.WriteSync)
            {
                if (!RemoveCore(item))
                    return false;

                this.OnChanged();

                foreach (var subscription in _observers)
                {
                    subscription.Observer.OnItemRemoved(subscription, item);
                }

                return true;
            }
        }

        public int Count => _items.Count;

        public bool IsReadOnly => false;
    }
}