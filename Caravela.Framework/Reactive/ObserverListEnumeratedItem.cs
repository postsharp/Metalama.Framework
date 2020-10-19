namespace Caravela.Reactive
{
    /// <summary>
    /// An item exposed by enumerators of <see cref="ObserverList{T}"/>.
    /// </summary>
    /// <typeparam name="T">The observer type.</typeparam>
    internal readonly struct ObserverListEnumeratedItem<T> where T : IReactiveObserver
    {
        public T Observer { get; }
        public IReactiveSubscription<T> Subscription { get; }

        internal ObserverListEnumeratedItem(T observer, IReactiveSubscription<T> subscription)
        {
            this.Observer = observer;
            this.Subscription = subscription;
        }

        public override string ToString() => this.Observer.ToString();

    }
}