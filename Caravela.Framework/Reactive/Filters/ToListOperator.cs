#region

using System.Collections.Generic;
using System.Collections.Immutable;

#endregion

namespace Caravela.Reactive
{
    internal class ToListOperator<T> : ReactiveCollectionOperator<T, T>
    {
        private ImmutableList<T> _list;

        public ToListOperator(IReactiveCollection<T> source) : base(source)
        {
        }

        public override bool IsMaterialized => true;

        protected override bool EvaluateFunction(IEnumerable<T> source)
        {
            this._list = ImmutableList.CreateRange(source);
            return true;
        }

        protected override IEnumerable<T> GetFunctionResult()
        {
            return this._list;
        }


        protected override void OnSourceItemAdded(IReactiveSubscription sourceSubscription, T item,
            in UpdateToken updateToken)
        {
            var oldList = this._list;
            
            this._list = this._list.Add(item);

            updateToken.SignalChange();

            foreach (var subscription in this.Observers)
            {
                subscription.Observer.OnItemAdded(subscription.Subscription, item, updateToken.Version);
            }
            
            foreach (var subscription in this.Observers.OfType<IEnumerable<T>>())
            {
                subscription.Observer.OnValueChanged(subscription.Subscription, oldList, this._list, updateToken.Version);
            }
        }

        protected override void OnSourceItemRemoved(IReactiveSubscription sourceSubscription, T item,
            in UpdateToken updateToken)
        {
            var oldList = this._list;
            
            this._list = this._list.Remove(item);

            updateToken.SignalChange();

            foreach (var subscription in this.Observers)
            {
                subscription.Observer.OnItemRemoved(subscription.Subscription, item, updateToken.Version);
            }
            
            foreach (var subscription in this.Observers.OfType<IEnumerable<T>>())
            {
                subscription.Observer.OnValueChanged(subscription.Subscription, oldList, this._list, updateToken.Version);
            }
        }

        protected override void OnSourceItemReplaced(IReactiveSubscription sourceSubscription, T oldItem, T newItem,
            in UpdateToken updateToken)
        {
            var oldList = this._list;
            
            this._list = this._list.Replace(oldItem, newItem);

            updateToken.SignalChange();

            foreach (var subscription in this.Observers)
            {
                subscription.Observer.OnItemReplaced(subscription.Subscription, oldItem, newItem, updateToken.Version);
            }
            
            foreach (var subscription in this.Observers.OfType<IEnumerable<T>>())
            {
                subscription.Observer.OnValueChanged(subscription.Subscription, oldList, this._list, updateToken.Version);
            }
        }
    }
}