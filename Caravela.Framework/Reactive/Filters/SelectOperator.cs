using System;
using System.Collections.Generic;
using System.Linq;

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
            _func = func;
        }

        protected override bool EvaluateFunction(IEnumerable<TSource> source)
        {
            _results = source.Select(s => _func(s, CollectorToken));
            return true;
        }

        protected override IEnumerable<TResult> GetFunctionResult()
        {
            return _results;
        }

        protected override void OnSourceItemAdded(IReactiveSubscription sourceSubscription, TSource item,
            in UpdateToken updateToken)
        {
            var outItem = _func(item, CollectorToken);

            updateToken.SignalChange();

            foreach (var subscription in Observers)
                subscription.Observer.OnItemAdded(subscription, outItem, updateToken.Version);
        }

        protected override void OnSourceItemRemoved(IReactiveSubscription sourceSubscription, TSource item,
            in UpdateToken updateToken)
        {
            var outItem = _func(item, CollectorToken);

            updateToken.SignalChange();

            foreach (var subscription in Observers)
                subscription.Observer.OnItemRemoved(subscription, outItem, updateToken.Version);
        }

        protected override void OnSourceItemReplaced(IReactiveSubscription sourceSubscription, TSource oldItem,
            TSource newItem, in UpdateToken updateToken)
        {
            if (_sourceEqualityComparer.Equals(oldItem, newItem)) return;

            var oldItemResult = _func(oldItem, CollectorToken);
            var newItemResult = _func(newItem, CollectorToken);

            if (_resultEqualityComparer.Equals(oldItemResult, newItemResult)) return;

            updateToken.SignalChange();

            foreach (var subscription in Observers)
            {
                subscription.Observer.OnItemRemoved(subscription, oldItemResult, updateToken.Version);
                subscription.Observer.OnItemAdded(subscription, newItemResult, updateToken.Version);
            }
        }
    }
}