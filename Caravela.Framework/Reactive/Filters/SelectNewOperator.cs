#region

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

#endregion

namespace Caravela.Reactive
{
    internal class SelectNewOperator<TSource, TResult> : ReactiveCollectionOperator<TSource, TResult>
        where TSource : class 
        where TResult : class
    {
        private static readonly IEqualityComparer<TSource> _sourceEqualityComparer =
            EqualityComparerFactory.GetEqualityComparer<TSource>();

        private static readonly IEqualityComparer<TResult> _resultEqualityComparer = EqualityComparer<TResult>.Default;
        private readonly Func<TSource, ReactiveCollectorToken, TResult> _func;
        private readonly ConditionalWeakTable<TSource,TResult> _map = new ConditionalWeakTable<TSource, TResult>();
        private IEnumerable<TResult> _results;

        public SelectNewOperator(IReactiveCollection<TSource> source, Func<TSource, ReactiveCollectorToken, TResult> func)
            : base(source)
        {
            this._func = func;
        }

        public override bool IsMaterialized => false;

        private TResult Selector(TSource s) => this._map.GetValue(s, k => this._func(k, this.CollectorToken));

        protected override bool EvaluateFunction(IEnumerable<TSource> source)
        {
            this._results = source.Select(this.Selector);
            
            return true;
        }

        protected override IEnumerable<TResult> GetFunctionResult()
        {
            Debug.Assert(this._results!=null);
            return this._results;
        }

        protected override void OnSourceItemAdded(IReactiveSubscription sourceSubscription, TSource item,
            in UpdateToken updateToken)
        {
            var outItem = this.Selector(item);

            updateToken.SignalChange(true);

            foreach (var subscription in this.Observers)
            {
                subscription.Observer.OnItemAdded(subscription.Subscription, outItem, updateToken.Version);
            }
        }

        protected override void OnSourceItemRemoved(IReactiveSubscription sourceSubscription, TSource item,
            in UpdateToken updateToken)
        {
            if (!this._map.TryGetValue(item, out var outItem))
            {
                return;
            }
            
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

            if (!this._map.TryGetValue(oldItem, out var oldItemResult))
            {
                // Should not happen.
                return;
            }
            

            var newItemResult = this.Selector(newItem);

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