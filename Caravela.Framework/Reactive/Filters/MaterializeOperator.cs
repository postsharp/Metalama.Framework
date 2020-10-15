using System.Collections.Generic;
using System.Collections.Immutable;

namespace Caravela.Reactive
{
    internal class MaterializeOperator<T>: ReactiveCollectionOperator<T, T>
    {
        private ImmutableList<T> _list = null!;
        public MaterializeOperator(IReactiveCollection<T> source) : base(source)
        {
        }

        protected override bool EvaluateFunction(IEnumerable<T> source)
        {
            _list = ImmutableList.CreateRange(source);
            return true;
        }

        protected override IEnumerable<T> GetFunctionResult() => _list;
        

        protected override void OnSourceItemAdded(IReactiveSubscription sourceSubscription, T item, in UpdateToken updateToken)
        {
            _list = _list.Add(item);
            
            updateToken.SignalChange();

            foreach (var subscription in Observers)
                subscription.Observer.OnItemAdded(subscription, item, updateToken.Version);
        }

        protected override void OnSourceItemRemoved(IReactiveSubscription sourceSubscription, T item, in UpdateToken updateToken)
        {
            _list = _list.Remove(item);
            
            updateToken.SignalChange();

            foreach (var subscription in Observers)
                subscription.Observer.OnItemRemoved(subscription, item, updateToken.Version);
        }

        protected override void OnSourceItemReplaced(IReactiveSubscription sourceSubscription, T oldItem, T newItem, in UpdateToken updateToken)
        {
            _list = _list.Replace(oldItem, newItem);
            
            updateToken.SignalChange();

            foreach (var subscription in Observers)
                subscription.Observer.OnItemReplaced(subscription, oldItem, newItem, updateToken.Version);
        }

        public override bool IsMaterialized => true;
    }
}