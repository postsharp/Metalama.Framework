#region

using System;

#endregion

namespace Caravela.Reactive
{
    /// <summary>
    /// An item of <see cref="ObserverList{T}"/>. Implements <see cref="IReactiveSubscription{T}"/>. 
    /// </summary>
    /// <typeparam name="T">Type of observer.</typeparam>
    internal sealed class ObserverListItem<T> : IReactiveSubscription<T>
        where T : IReactiveObserver
    {
        public IReactiveObserver WeaklyTypedObserver { get; }
        internal ObserverListItem<T> Next;

        public ObserverListItem(IReactiveObservable<T> source, IReactiveObserver observer)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            this.Sender = source;
            this.WeaklyTypedObserver = observer;
        }

        private IReactiveObservable<T> Sender { get; set; }

        IReactiveObserver IReactiveSubscription.Observer => this.WeaklyTypedObserver;

        object IReactiveSubscription.Observable => this.Sender;
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
            return this.WeaklyTypedObserver?.ToString() ?? "null";
        }
    }
}