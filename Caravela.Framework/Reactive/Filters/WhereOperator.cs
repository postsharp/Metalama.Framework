using System;
using System.Collections.Generic;
using System.Linq;

namespace Caravela.Reactive
{
    internal class WhereOperator<T> : ReactiveCollectionOperator<T, T>
    {
        private static readonly IEqualityComparer<T> _sourceEqualityComparer =
            EqualityComparerFactory.GetEqualityComparer<T>();

        private readonly Func<T, ReactiveCollectorToken, bool> _predicate;
        private IEnumerable<T> _result;

        public WhereOperator(IReactiveCollection<T> source, Func<T, ReactiveCollectorToken, bool> predicate) :
            base(source)
        {
            _predicate = predicate;
        }

        protected override bool EvaluateFunction(IEnumerable<T> source)
        {
            _result = source.Where(arg => _predicate(arg, CollectorToken));
            return true;
        }

        protected override IEnumerable<T> GetFunctionResult()
        {
            return _result;
        }

        protected override void OnSourceItemAdded(IReactiveSubscription sourceSubscription, T item,
            in UpdateToken updateToken)
        {
            if (_predicate(item, CollectorToken))
            {
                updateToken.SignalChange();

                foreach (var subscription in Observers)
                    subscription.Observer.OnItemAdded(subscription, item, updateToken.Version);
            }
        }

        protected override void OnSourceItemRemoved(IReactiveSubscription sourceSubscription, T item,
            in UpdateToken updateToken)
        {
            if (_predicate(item, CollectorToken))
            {
                updateToken.SignalChange();

                foreach (var subscription in Observers)
                    subscription.Observer.OnItemRemoved(subscription, item, updateToken.Version);
            }
        }

        protected override void OnSourceItemReplaced(IReactiveSubscription sourceSubscription, T oldItem, T newItem,
            in UpdateToken updateToken)
        {
            if (_sourceEqualityComparer.Equals(oldItem, newItem)) return;

            var remove = _predicate(oldItem, CollectorToken);
            var add = _predicate(newItem, CollectorToken);

            if (!remove && !add) return;

            updateToken.SignalChange();

            foreach (var subscription in Observers)
            {
                if (remove) subscription.Observer.OnItemRemoved(subscription, oldItem, updateToken.Version);

                if (add) subscription.Observer.OnItemRemoved(subscription, newItem, updateToken.Version);
            }
        }
    }
}