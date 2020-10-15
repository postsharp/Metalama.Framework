using System.Collections.Generic;
using System.Linq;

namespace Caravela.Reactive
{
    internal class UnionOperator<T> : ReactiveCollectionOperator<T, T>
    {
        private readonly IReactiveCollection<T> _second;
        private IEnumerable<T> _results = null!;
        private IReactiveSubscription? _secondSubscription;

        public UnionOperator(IReactiveCollection<T> source, IReactiveCollection<T> second)
            : base(source)
        {
            _second = second;
        }

        protected override bool EvaluateFunction(IEnumerable<T> source)
        {
            _results = source.Union(_second.GetValue(CollectorToken));
            return true;
        }

        protected internal override IReactiveSubscription SubscribeToSource()
        {
            _secondSubscription = _second.AddObserver(this);
            return base.SubscribeToSource();
        }

        protected internal override void UnsubscribeFromSource()
        {
            base.UnsubscribeFromSource();
            _secondSubscription?.Dispose();
            _secondSubscription = null;
        }

        protected override IEnumerable<T> GetFunctionResult()
        {
            return _results;
        }

        protected override void OnSourceItemAdded(IReactiveSubscription sourceSubscription, T item,
            in UpdateToken updateToken)
        {
            updateToken.SignalChange();

            foreach (var subscription in Observers)
                subscription.Observer.OnItemAdded(subscription, item, updateToken.Version);
        }

        protected override void OnSourceItemRemoved(IReactiveSubscription sourceSubscription, T item,
            in UpdateToken updateToken)
        {
            updateToken.SignalChange();

            foreach (var subscription in Observers)
                subscription.Observer.OnItemRemoved(subscription, item, updateToken.Version);
        }

        protected override void OnSourceItemReplaced(IReactiveSubscription sourceSubscription, T oldItem, T newItem,
            in UpdateToken updateToken)
        {
            updateToken.SignalChange();

            foreach (var subscription in Observers)
            {
                subscription.Observer.OnItemRemoved(subscription, oldItem, updateToken.Version);
                subscription.Observer.OnItemAdded(subscription, newItem, updateToken.Version);
            }
        }
    }
}