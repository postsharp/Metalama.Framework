#region

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

#endregion

namespace Caravela.Reactive
{
    internal class SelectImpureOperator<TSource, TResult> : ReactiveCollectionOperator<TSource, TResult>
    {
        private static readonly IEqualityComparer<TSource> _sourceEqualityComparer =
            EqualityComparerFactory.GetEqualityComparer<TSource>();

        private static readonly IEqualityComparer<TResult> _resultEqualityComparer = EqualityComparer<TResult>.Default;
        private readonly Func<TSource, ReactiveCollectorToken, TResult> _func;
        private ImmutableDictionary<TSource,TResult> _results;

        public SelectImpureOperator(IReactiveCollection<TSource> source, Func<TSource, ReactiveCollectorToken, TResult> func)
            : base(source)
        {
            this._func = func;
        }

        public override bool IsMaterialized => true;


        protected override bool EvaluateFunction(IEnumerable<TSource> source)
        {
            this._results = source.ToImmutableDictionary(s => s,
                s => this._func(s, this.CollectorToken), _sourceEqualityComparer);
            
            return true;
        }

        protected override IEnumerable<TResult> GetFunctionResult()
        {
            return this._results.Values;
        }

        protected override void OnSourceItemAdded(IReactiveSubscription sourceSubscription, TSource item,
            in UpdateToken updateToken)
        {
            var outItem = this._func(item, this.CollectorToken);

            this._results = this._results.Add(item, outItem);

            updateToken.SignalChange();

            foreach (var subscription in this.Observers)
            {
                subscription.Observer.OnItemAdded(subscription.Subscription, outItem, updateToken.Version);
            }
        }

        protected override void OnSourceItemRemoved(IReactiveSubscription sourceSubscription, TSource item,
            in UpdateToken updateToken)
        {
            if (!this._results.TryGetValue(item, out var outItem))
            {
                return;
            }
            
            this._results = this._results.Remove(item);

            updateToken.SignalChange();

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

            if (!this._results.TryGetValue(oldItem, out var oldItemResult))
            {
                // Should not happen.
                return;
            }
            
            

            var newItemResult = this._func(newItem, this.CollectorToken);

            if (_resultEqualityComparer.Equals(oldItemResult, newItemResult))
            {
                return;
            }
            
                        
            this._results = this._results.Remove(oldItem).Add(newItem, newItemResult);
            
            updateToken.SignalChange();

            foreach (var subscription in this.Observers)
            {
                subscription.Observer.OnItemRemoved(subscription.Subscription, oldItemResult, updateToken.Version);
                subscription.Observer.OnItemAdded(subscription.Subscription, newItemResult, updateToken.Version);
            }
        }
    }
}