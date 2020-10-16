#region

using System;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace Caravela.Reactive
{
    internal class SelectOperator<TSource, TResult> : ReactiveCollectionOperator<TSource, TResult>
    {
        private static readonly IEqualityComparer<TSource> _sourceEqualityComparer =
            EqualityComparerFactory.GetEqualityComparer<TSource>();

        private static readonly IEqualityComparer<TResult> _resultEqualityComparer = EqualityComparer<TResult>.Default;
        private readonly Func<TSource, ReactiveCollectorToken, TResult> _func;
        private IEnumerable<TResult> _results;

        public SelectOperator(IReactiveCollection<TSource> source, Func<TSource, ReactiveCollectorToken, TResult> func)
            : base(source)
        {
            this._func = func;
        }

        protected override bool EvaluateFunction(IEnumerable<TSource> source)
        {
            this._results = source.Select(s => this._func(s, this.CollectorToken));
            return true;
        }

        protected override IEnumerable<TResult> GetFunctionResult()
        {
            return this._results;
        }

        protected override void OnSourceItemAdded(IReactiveSubscription sourceSubscription, TSource item,
            in UpdateToken updateToken)
        {
            var outItem = this._func(item, this.CollectorToken);

            updateToken.SignalChange(true);

            foreach (var subscription in this.Observers)
            {
                subscription.Observer.OnItemAdded(subscription.Subscription, outItem, updateToken.Version);
            }
        }

        protected override void OnSourceItemRemoved(IReactiveSubscription sourceSubscription, TSource item,
            in UpdateToken updateToken)
        {
            var outItem = this._func(item, this.CollectorToken);

            updateToken.SignalChange(true);

            foreach (var subscription in this.Observers)
            {
                subscription.Observer.OnItemRemoved(subscription.Subscription, outItem, updateToken.Version);
            }
        }

        protected override void OnSourceItemReplaced(IReactiveSubscription sourceSubscription, TSource oldItem,
            TSource newItem, in UpdateToken updateToken)
        {
            if (_sourceEqualityComparer.Equals(oldItem, newItem))
            {
                return;
            }

            var oldItemResult = this._func(oldItem, this.CollectorToken);
            var newItemResult = this._func(newItem, this.CollectorToken);

            if (_resultEqualityComparer.Equals(oldItemResult, newItemResult))
            {
                return;
            }

            updateToken.SignalChange(true);

            foreach (var subscription in this.Observers)
            {
                subscription.Observer.OnItemRemoved(subscription.Subscription, oldItemResult, updateToken.Version);
                subscription.Observer.OnItemAdded(subscription.Subscription, newItemResult, updateToken.Version);
            }
        }
    }
}