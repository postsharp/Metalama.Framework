using System;
using System.Collections.Generic;

namespace Caravela.Reactive
{
    internal class ForEachOperator<T> : IReactiveCollectionObserver<T>, IReactiveTokenCollector
    {
        private readonly Action<T>? _removeAction;
        private readonly Action<T>? _addAction;
        private readonly IReactiveCollection<T> _source;
        private readonly IReactiveSubscription _subscription;

        public ForEachOperator(IReactiveCollection<T> source, Action<T>? addAction, Action<T>? removeAction)
        {
            _addAction = addAction;
            _removeAction = removeAction;
            _source = source;
            _subscription = _source.AddObserver(this);

            foreach (var item in _source.GetValue(new ReactiveCollectorToken(this)))
                _addAction?.Invoke(item);
        }

        public void Dispose()
        {
            _subscription.Dispose();
        }

        void IReactiveCollectionObserver<T>.OnItemAdded(IReactiveSubscription subscription, T item, int newVersion)
        {
            _addAction?.Invoke(item);
        }

        void IReactiveCollectionObserver<T>.OnItemRemoved(IReactiveSubscription subscription, T item, int newVersion)
        {
            _removeAction?.Invoke(item);
        }

        void IReactiveCollectionObserver<T>.OnItemReplaced(IReactiveSubscription subscription, T oldItem, T newItem,
            int newVersion)
        {
            _addAction?.Invoke(newItem);
            _removeAction?.Invoke(oldItem);
        }

        void IReactiveObserver.OnValueInvalidated(IReactiveSubscription subscription, bool isBreakingChange)
        {
        }

        void IReactiveObserver<IEnumerable<T>>.OnValueChanged(IReactiveSubscription subscription,
            IEnumerable<T> oldValue, IEnumerable<T> newValue, int newVersion,
            bool isBreakingChange)
        {
        }

        void IReactiveTokenCollector.AddDependency(IReactiveObservable<IReactiveObserver> observable)
        {
            if (observable != this) throw new InvalidOperationException();
        }
    }
}