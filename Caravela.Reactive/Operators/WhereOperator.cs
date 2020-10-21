#region

using System;
using System.Collections.Generic;
using System.Linq;
using Caravela.Reactive.Implementation;

#endregion

namespace Caravela.Reactive.Operators
{
    internal class WhereOperator<T> : ReactiveCollectionOperator<T, T>
    {
        private static readonly IEqualityComparer<T> _sourceEqualityComparer =
            EqualityComparerFactory.GetEqualityComparer<T>();

        private readonly Func<T, ReactiveCollectorToken, bool> _predicate;

        public WhereOperator(IReactiveCollection<T> source, Func<T, bool> predicate) :
            base(source)
        {
            this._predicate = ReactiveCollectorToken.WrapWithDefaultToken(predicate);
        }

        protected override ReactiveOperatorResult<IEnumerable<T>> EvaluateFunction(IEnumerable<T> source)
        {
            return new(source.Where(arg => this._predicate(arg, this.ObserverToken)));
        }

        protected override void OnSourceItemAdded(IReactiveSubscription sourceSubscription, T item,
            in IncrementalUpdateToken updateToken)
        {
            if (this._predicate(item, this.ObserverToken))
            {
                updateToken.SetBreakingChange();

                foreach (var subscription in this.Observers)
                {
                    subscription.Observer.OnItemAdded(subscription.Subscription, item, updateToken.NextVersion);
                }
            }
        }

        protected override void OnSourceItemRemoved(IReactiveSubscription sourceSubscription, T item,
            in IncrementalUpdateToken updateToken)
        {
            if (this._predicate(item, this.ObserverToken))
            {
                updateToken.SetBreakingChange();

                foreach (var subscription in this.Observers)
                {
                    subscription.Observer.OnItemRemoved(subscription.Subscription, item, updateToken.NextVersion);
                }
            }
        }

        protected override void OnSourceItemReplaced(IReactiveSubscription sourceSubscription, T oldItem, T newItem,
            in IncrementalUpdateToken updateToken)
        {
            if (_sourceEqualityComparer.Equals(oldItem, newItem))
            {
                return;
            }

            var remove = this._predicate(oldItem, this.ObserverToken);
            var add = this._predicate(newItem, this.ObserverToken);

            if (!remove && !add)
            {
                return;
            }

            updateToken.SetBreakingChange();

            foreach (var subscription in this.Observers)
            {
                if (remove)
                {
                    subscription.Observer.OnItemRemoved(subscription.Subscription, oldItem, updateToken.NextVersion);
                }

                if (add)
                {
                    subscription.Observer.OnItemRemoved(subscription.Subscription, newItem, updateToken.NextVersion);
                }
            }
        }
    }
}