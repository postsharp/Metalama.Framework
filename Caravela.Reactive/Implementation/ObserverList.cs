#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

#endregion

namespace Caravela.Reactive.Implementation
{
    /// <summary>
    /// A base implementation for <see cref="IReactiveObservable{T}"/>.
    /// </summary>
    /// <typeparam name="T">Type of observers/</typeparam>
    public struct ObserverList<T> : IEnumerable<ObserverListEnumeratedItem<T>>
        where T : class, IReactiveObserver
    {
        private SpinLock _spinLock;
        private readonly IReactiveObservable<T> _owner;
        private volatile ObserverListItem<T>? _first;

        public ObserverList(IReactiveObservable<T> owner)
        {
            this._owner = owner;
            this._first = null;
            this._spinLock = default;
        }

        public bool IsEmpty => this._first == null;


        public ObserverListEnumerator<T, IReactiveObserver> WeaklyTyped()
            => new ObserverListEnumerator<T, IReactiveObserver>(this._first);

        public ObserverListEnumerator<T, IReactiveObserver<TOut>> OfType<TOut>()
            => new ObserverListEnumerator<T, IReactiveObserver<TOut>>(this._first);

        IEnumerator<ObserverListEnumeratedItem<T>> IEnumerable<ObserverListEnumeratedItem<T>>.GetEnumerator() => this.GetEnumerator();
       
        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        public ObserverListEnumerator<T, T> GetEnumerator()
        {
            return new ObserverListEnumerator<T, T>(this._first);
        }

        private bool ContainsObserver(IReactiveObserver observer)
        {
            for (var node = this._first; node != null; node = node.Next)
            {
                if (ReferenceEquals(node.Observer, observer))
                {
                    return true;
                }
            }

            return false;
        }

        private int Count
        {
            get
            {
                var count = 0;
                for (var node = this._first; node != null; node = node.Next)
                {
                    count++;
                }

                return count;
            }
        }

        public IReactiveSubscription<T>? AddObserver(IReactiveObserver observer)
        {
            if (observer == null)
            {
                throw new ArgumentNullException();
            }

            // this happens when SelectManyRecursive uses its Follow and then GetValue
            if ( this.ContainsObserver( observer ) )
            {
                return null;
            }

            var node = new ObserverListItem<T>(this._owner, observer);

            var lockHeld = false;
            try
            {
                this._spinLock.Enter(ref lockHeld);

                node.Next = this._first;
                this._first = node;
                return node;
            }
            finally
            {
                if (lockHeld)
                {
                    this._spinLock.Exit();
                }
            }
        }

        public bool RemoveObserver(IReactiveSubscription subscription)
        {
            var lockHeld = false;
            try
            {
                this._spinLock.Enter(ref lockHeld);

                ObserverListItem<T>? previous = null;
                for (var node = this._first; node != null; node = node.Next)
                {
                    if (ReferenceEquals(node, subscription))
                    {
                        if (previous == null)
                        {
                            this._first = node.Next;
                        }
                        else
                        {
                            previous.Next = node.Next;
                        }

                        return true;
                    }

                    previous = node;
                }
            }
            finally
            {
                if (lockHeld)
                {
                    this._spinLock.Exit();
                }
            }

            // Not found.
            return false;
        }


        public override string ToString() => $"ObserverList Count={this.Count}";
    }
}