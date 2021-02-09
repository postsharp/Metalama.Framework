using System.Collections;
using System.Collections.Generic;

namespace Caravela.Reactive.Implementation
{
    /// <summary>
    /// Represents an iterators returned by <see cref="ObserverList{T}"/>.
    /// This struct exists to perform type conversions.
    /// </summary>
    /// <typeparam name="TIn">Type of observers stored in the <see cref="ObserverList{T}"/>.</typeparam>
    /// <typeparam name="TOut">Type of observers requested by the consumer.</typeparam>
    public struct ObserverListEnumerator<TIn, TOut> :
        IEnumerable<ObserverListEnumeratedItem<TOut>>,
        IEnumerator<ObserverListEnumeratedItem<TOut>>
        where TOut : class, IReactiveObserver
        where TIn : class, IReactiveObserver
    {
        private readonly ObserverListItem<TIn>? _first;
        private ObserverListItem<TIn>? _node;

        internal ObserverListEnumerator( ObserverListItem<TIn>? first )
        {
            this._first = first;
            this._node = null;
        }

        public ObserverListEnumerator<TIn, T> OfType<T>()
            where T : class, IReactiveObserver
            => new ObserverListEnumerator<TIn, T>( this._first );

        public ObserverListEnumerator<TIn, IReactiveObserver<IEnumerable<T>>> OfEnumerableType<T>()
            => new ObserverListEnumerator<TIn, IReactiveObserver<IEnumerable<T>>>( this._first );

        public void Dispose()
        {
        }

        public bool MoveNext()
        {

            while ( true )
            {
                this._node = this._node == null ? this._first : this._node.Next;

                if ( this._node == null )
                {
                    return false;
                }

                if ( this._node.WeaklyTypedObserver is TOut )
                {
                    return true;
                }
            }
        }

        public void Reset()
        {
            this._node = null;
        }

        public ObserverListEnumeratedItem<TOut> Current =>
            new ObserverListEnumeratedItem<TOut>( (TOut) this._node!.WeaklyTypedObserver, (IReactiveSubscription<TOut>) (object) this._node );

        object IEnumerator.Current => this.Current;

        public ObserverListEnumerator<TIn, TOut> GetEnumerator()
        {
            return this;
        }

        IEnumerator<ObserverListEnumeratedItem<TOut>> IEnumerable<ObserverListEnumeratedItem<TOut>>.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}