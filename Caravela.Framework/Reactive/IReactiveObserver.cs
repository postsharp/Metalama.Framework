using System;
using System.Runtime.CompilerServices;

namespace Caravela.Reactive
{
    public interface IReactiveObserver : IDisposable
    {
        // Called when changes cannot be propagated to observers incrementally, and a full resync is necessary.
        void OnReset(IReactiveSubscription subscription);
    }

    public interface IReactiveCollectionObserver<in T> : IReactiveObserver
    {
        void OnItemAdded(IReactiveSubscription subscription, T item);
        void OnItemRemoved(IReactiveSubscription subscription, T item);
        void OnItemReplaced(IReactiveSubscription subscription, T oldItem, T newItem);
        void OnItemChanged(IReactiveSubscription subscription, T item);
    }
}