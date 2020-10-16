#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

#endregion

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
            this._predicate = predicate;
        }

        protected override bool EvaluateFunction(IEnumerable<T> source)
        {
            this._result = source.Where(arg => this._predicate(arg, this.CollectorToken));
            return true;
        }

        protected override IEnumerable<T> GetFunctionResult()
        {
            Debug.Assert(this._result!=null);
            return this._result;
        }

        protected override void OnSourceItemAdded(IReactiveSubscription sourceSubscription, T item,
            in UpdateToken updateToken)
        {
            if (this._predicate(item, this.CollectorToken))
            {
                updateToken.SignalChange(true);

                foreach (var subscription in this.Observers)
                {
                    subscription.Observer.OnItemAdded(subscription.Subscription, item, updateToken.Version);
                }
            }
        }

        protected override void OnSourceItemRemoved(IReactiveSubscription sourceSubscription, T item,
            in UpdateToken updateToken)
        {
            if (this._predicate(item, this.CollectorToken))
            {
                updateToken.SignalChange(true);

                foreach (var subscription in this.Observers)
                {
                    subscription.Observer.OnItemRemoved(subscription.Subscription, item, updateToken.Version);
                }
            }
        }

        protected override void OnSourceItemReplaced(IReactiveSubscription sourceSubscription, T oldItem, T newItem,
            in UpdateToken updateToken)
        {
            if (_sourceEqualityComparer.Equals(oldItem, newItem))
            {
                return;
            }

            var remove = this._predicate(oldItem, this.CollectorToken);
            var add = this._predicate(newItem, this.CollectorToken);

            if (!remove && !add)
            {
                return;
            }

            updateToken.SignalChange(true);

            foreach (var subscription in this.Observers)
            {
                if (remove)
                {
                    subscription.Observer.OnItemRemoved(subscription.Subscription, oldItem, updateToken.Version);
                }

                if (add)
                {
                    subscription.Observer.OnItemRemoved(subscription.Subscription, newItem, updateToken.Version);
                }
            }
        }
    }
}