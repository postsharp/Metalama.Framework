#region

using System.Collections.Generic;
using System.Linq;

#endregion

namespace Caravela.Reactive
{
    internal class UnionOperator<T> : ReactiveCollectionOperator<T, T>
    {
        private readonly IReactiveCollection<T> _second;
        private IEnumerable<T> _results;
        private IReactiveSubscription _secondSubscription;

        public UnionOperator(IReactiveCollection<T> source, IReactiveCollection<T> second)
            : base(source)
        {
            this._second = second;
        }

        protected override bool EvaluateFunction(IEnumerable<T> source)
        {
            this._results = source.Union(this._second.GetValue(this.CollectorToken));
            return true;
        }

        protected internal override IReactiveSubscription SubscribeToSource()
        {
            this._secondSubscription = this._second.AddObserver(this);
            return base.SubscribeToSource();
        }

        protected internal override void UnsubscribeFromSource()
        {
            base.UnsubscribeFromSource();
            this._secondSubscription?.Dispose();
            this._secondSubscription = null;
        }

        protected override IEnumerable<T> GetFunctionResult()
        {
            return this._results;
        }

        protected override void OnSourceItemAdded(IReactiveSubscription sourceSubscription, T item,
            in UpdateToken updateToken)
        {
            updateToken.SignalChange();

            foreach (var subscription in this.Observers)
            {
                subscription.Observer.OnItemAdded(subscription.Subscription, item, updateToken.Version);
            }
        }

        protected override void OnSourceItemRemoved(IReactiveSubscription sourceSubscription, T item,
            in UpdateToken updateToken)
        {
            updateToken.SignalChange();

            foreach (var subscription in this.Observers)
            {
                subscription.Observer.OnItemRemoved(subscription.Subscription, item, updateToken.Version);
            }
        }

        protected override void OnSourceItemReplaced(IReactiveSubscription sourceSubscription, T oldItem, T newItem,
            in UpdateToken updateToken)
        {
            updateToken.SignalChange();

            foreach (var subscription in this.Observers)
            {
                subscription.Observer.OnItemRemoved(subscription.Subscription, oldItem, updateToken.Version);
                subscription.Observer.OnItemAdded(subscription.Subscription, newItem, updateToken.Version);
            }
        }

        protected override bool ShouldTrackDependency(IReactiveObservable<IReactiveObserver> observable)
            => base.ShouldTrackDependency(observable) && observable.Object != this._second;
    }
}