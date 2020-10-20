#region

using System;
using System.Collections.Generic;
using System.Linq;
using Caravela.Reactive.Implementation;

#endregion

namespace Caravela.Reactive.Operators
{
    internal class SelectOperator<TSource, TResult> : ReactiveCollectionOperator<TSource, TResult>
    {
        private static readonly IEqualityComparer<TSource> _sourceEqualityComparer =
            EqualityComparerFactory.GetEqualityComparer<TSource>();

        private static readonly IEqualityComparer<TResult> _resultEqualityComparer = EqualityComparer<TResult>.Default;
        private readonly Func<TSource, ReactiveObserverToken, TResult> _func;

        public SelectOperator(IReactiveCollection<TSource> source, Func<TSource,  TResult> func)
            : base(source)
        {
            this._func = ReactiveObserverToken.WrapWithDefaultToken(func);
        }

        protected override IEnumerable<TResult> EvaluateFunction(IEnumerable<TSource> source)
        {
            return source.Select(s => this._func(s, this.ObserverToken));
        }

        protected override void OnSourceItemAdded(IReactiveSubscription sourceSubscription, TSource item,
            in IncrementalUpdateToken updateToken)
        {
            var outItem = this._func(item, this.ObserverToken);

            updateToken.SetBreakingChange();

            foreach (var subscription in this.Observers)
            {
                subscription.Observer.OnItemAdded(subscription.Subscription, outItem, updateToken.NextVersion);
            }
        }

        protected override void OnSourceItemRemoved(IReactiveSubscription sourceSubscription, TSource item,
            in IncrementalUpdateToken updateToken)
        {
            var outItem = this._func(item, this.ObserverToken);

            updateToken.SetBreakingChange();

            foreach (var subscription in this.Observers)
            {
                subscription.Observer.OnItemRemoved(subscription.Subscription, outItem, updateToken.NextVersion);
            }
        }

        protected override void OnSourceItemReplaced(IReactiveSubscription sourceSubscription, TSource oldItem,
            TSource newItem, in IncrementalUpdateToken updateToken)
        {
            if (_sourceEqualityComparer.Equals(oldItem, newItem))
            {
                return;
            }

            var oldItemResult = this._func(oldItem, this.ObserverToken);
            var newItemResult = this._func(newItem, this.ObserverToken);

            if (_resultEqualityComparer.Equals(oldItemResult, newItemResult))
            {
                return;
            }

            updateToken.SetBreakingChange();

            foreach (var subscription in this.Observers)
            {
                subscription.Observer.OnItemRemoved(subscription.Subscription, oldItemResult, updateToken.NextVersion);
                subscription.Observer.OnItemAdded(subscription.Subscription, newItemResult, updateToken.NextVersion);
            }
        }
    }
}