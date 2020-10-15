using System;
using System.Collections;
using System.Collections.Generic;

namespace Caravela.Reactive
{
    internal class ObserverList<T>  : IEnumerable<IReactiveSubscription<T>>
        where T : IReactiveObserver
    {
        private volatile Node? _first;
        private readonly IReactiveObservable<T> _owner;

        public ObserverList(IReactiveObservable<T> owner)
        {
            _owner = owner;
            _first = null;
        }

        public bool IsEmpty => _first == null;
     
        public IReactiveSubscription<T> AddObserver(T observer)
        {
            var node = new Node(_owner, observer);

            lock (this)
            {
                node.Next = _first;
                _first = node;
                return node;
            }
        }

        public bool RemoveObserver(IReactiveSubscription subscription)
        {
            lock ( this )
            {
                Node? previous = null;
                for (Node? node = _first; node != null; node = node.Next)
                {
                    if (ReferenceEquals(node, subscription))
                    {
                        
                        if (previous == null)
                        {
                            _first = node.Next;
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
           
            // Not found.
            return false;
        }

        internal class Node : IReactiveSubscription<T>
        {
            public IReactiveObservable<T> Sender { get; private set; }

            object IReactiveSubscription.Sender => Sender;

            public Node( IReactiveObservable<T> observable, T observer)
            {
                if ( observable == null )
                    throw new ArgumentNullException(nameof(observable));
                Sender = observable;
                Observer = observer;
            }

            public Node? Next;
            public  T Observer { get; }

            public void Dispose()
            {
                if (Sender != null)
                {
                    Sender.RemoveObserver(this);
                    Sender = null!;
                }
            }

            IReactiveObserver IReactiveSubscription.Observer => Observer;
        }

        public struct Enumerator : IEnumerator<IReactiveSubscription<T>>
        {
            private Node? _node;
            private readonly Node? _first;

            internal Enumerator(Node? first)
            {
                _first = first;
                _node = null;
            }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                if (_node == null)
                {
                    _node = _first;
                }
                else
                {
                    _node = _node.Next;
                }

                return _node != null;
            }

            public void Reset()
            {
                _node = null;
            }

            public IReactiveSubscription<T> Current => _node!;

            object IEnumerator.Current => Current;
        }
        
        public Enumerator GetEnumerator() => new Enumerator(_first);

        IEnumerator<IReactiveSubscription<T>> IEnumerable<IReactiveSubscription<T>>.GetEnumerator() => GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}