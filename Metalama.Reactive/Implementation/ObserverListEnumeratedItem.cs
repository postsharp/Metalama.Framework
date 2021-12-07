// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Metalama.Reactive.Implementation
{
    /// <summary>
    /// An item exposed by enumerators of <see cref="ObserverList{T}"/>.
    /// </summary>
    /// <typeparam name="T">The observer type.</typeparam>
    public readonly struct ObserverListEnumeratedItem<T>
        where T : IReactiveObserver
    {
        public T Observer { get; }

        public IReactiveSubscription<T> Subscription { get; }

        internal ObserverListEnumeratedItem( T observer, IReactiveSubscription<T> subscription )
        {
            this.Observer = observer;
            this.Subscription = subscription;
        }

        public override string ToString() => this.Observer.ToString();
    }
}