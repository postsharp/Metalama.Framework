#region

using System;
using System.Collections;
using System.Collections.Generic;

#endregion

namespace Caravela.Reactive
{
    internal class ObserverList<T> : IEnumerable<ReactiveEnumeratedSubscription<T>>
        where T : class, IReactiveObserver
    {
        private readonly IReactiveObservable<T> _owner;
        private volatile ReactiveSubscription<T> _first;

        public ObserverList(IReactiveObservable<T> owner)
        {
            this._owner = owner;
            this._first = null;
        }

        public bool IsEmpty => this._first == null;


        public SubscriptionEnumerator<T, IReactiveObserver> WeaklyTyped()
            => new SubscriptionEnumerator<T, IReactiveObserver>(this._first);

        public SubscriptionEnumerator<T, IReactiveObserver<TOut>> OfType<TOut>()
            => new SubscriptionEnumerator<T, IReactiveObserver<TOut>>(this._first);

        IEnumerator<ReactiveEnumeratedSubscription<T>> IEnumerable<ReactiveEnumeratedSubscription<T>>.GetEnumerator()
        {
            return new SubscriptionEnumerator<T, T>(this._first);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new SubscriptionEnumerator<T, T>(this._first);
        }

        string Simplify(object t) => t.ToString().Replace("Caravela.Reactive.", "").Replace("System.", "")
            .Replace("TestModel.", "");


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
                int count = 0;
                for (var node = this._first; node != null; node = node.Next)
                {
                    count++;
                }

                return count;
            }
        }

        public IReactiveSubscription<T> AddObserver(IReactiveObserver observer)
        {
            if (observer == null)
                throw new ArgumentNullException();
            
            #if DEBUG
            if ( this.ContainsObserver(observer))
                throw new InvalidOperationException();
            
            #endif

            var node = new ReactiveSubscription<T>(this._owner, observer);

            lock (this)
            {
                node.Next = this._first;
                this._first = node;
                return node;
            }
        }

        public bool RemoveObserver(IReactiveSubscription subscription)
        {
            lock (this)
            {
                ReactiveSubscription<T> previous = null;
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

            // Not found.
            return false;
        }

        private static int indent;

        public bool HasPathToObserver(object source)
        {
            foreach (var node in this.WeaklyTyped())
            {
                string indentString = new string(' ', indent * 3);

                Console.WriteLine($"{indentString}HasPathToObserver({ this.Simplify(node.Observer)}, {this.Simplify(source)}");
                indent++;


                if (node.Observer == source)
                {
                    indent--;
                    Console.WriteLine($"{indentString}HasPathToObserver({this.Simplify(node.Observer)}, {this.Simplify(source)} -> TRUE");
                    return true;
                }
                else
                {
                    var result = node.Observer.HasPathToSource(source);
                    indent--;
                    Console.WriteLine($"{indentString}HasPathToObserver({this.Simplify(node.Observer)}, {this.Simplify(source)} -> {result}");
                    if (result)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public override string ToString() => $"ObserverList Count={Count}";

    }

    public struct ReactiveEnumeratedSubscription<T> where T : IReactiveObserver
    {
        public T Observer { get; }
        public IReactiveSubscription<T> Subscription { get; }

        internal ReactiveEnumeratedSubscription(T observer, IReactiveSubscription<T> subscription)
        {
            this.Observer = observer;
            this.Subscription = subscription;
        }

        public override string ToString() => this.Observer.ToString();

    }


    public struct SubscriptionEnumerator<TIn, TOut> : 
        IEnumerable<ReactiveEnumeratedSubscription<TOut>>,
        IEnumerator<ReactiveEnumeratedSubscription<TOut>>
        where TOut : class, IReactiveObserver
        where TIn : class, IReactiveObserver
    {
        private ReactiveSubscription<TIn> _node;
        private readonly ReactiveSubscription<TIn> _first;

        internal SubscriptionEnumerator(ReactiveSubscription<TIn> first)
        {
            this._first = first;
            this._node = null;
            
        }

        public void Dispose()
        {
        }

        public bool MoveNext()
        {
         
            while (true)
            {
                if (this._node == null)
                {
                    this._node = this._first;
                }
                else
                {
                    this._node = this._node.Next;
                }

                if (this._node == null)
                {
                    return false;
                }

                if (this._node.WeaklyTypedObserver is TOut)
                {
                    return true;
                }
            }
        }

        public void Reset()
        {
            this._node = null;
        }

        public ReactiveEnumeratedSubscription<TOut> Current =>
            new ReactiveEnumeratedSubscription<TOut>((TOut) this._node.WeaklyTypedObserver, (IReactiveSubscription<TOut>) this._node);

        object IEnumerator.Current => this.Current;

        public SubscriptionEnumerator<TIn, TOut> GetEnumerator()
        {
            return this;
        }

        IEnumerator<ReactiveEnumeratedSubscription<TOut>> IEnumerable<ReactiveEnumeratedSubscription<TOut>>.GetEnumerator()
        {
            return this.GetEnumerator();
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }


    internal class ReactiveSubscription<T> : IReactiveSubscription<T>
        where T : IReactiveObserver
    {
        public IReactiveObserver WeaklyTypedObserver { get; }
        internal ReactiveSubscription<T> Next;

        public ReactiveSubscription(IReactiveObservable<T> observable, IReactiveObserver observer)
        {
            if (observable == null)
            {
                throw new ArgumentNullException(nameof(observable));
            }

            this.Sender = observable;
            this.WeaklyTypedObserver = observer;
        }

        private IReactiveObservable<T> Sender { get; set; }

        IReactiveObserver IReactiveSubscription.Observer => this.WeaklyTypedObserver;

        object IReactiveSubscription.Sender => this.Sender;
        public T Observer => (T) this.WeaklyTypedObserver;

        public void Dispose()
        {
            if (this.Sender != null)
            {
                this.Sender.RemoveObserver(this);
                this.Sender = null;
            }
        }

        public override string ToString()
        {
            return this.WeaklyTypedObserver?.ToString();
        }
    }
}