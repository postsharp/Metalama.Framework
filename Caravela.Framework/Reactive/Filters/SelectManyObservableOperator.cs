using System;
using System.Collections.Generic;

namespace Caravela.Reactive
{
    internal class SelectManyObservableOperator<TSource, TResult> : SelectManyOperator<TSource, TResult>
    {
        private readonly Func<TSource, ReactiveCollectorToken, IReactiveCollection<TResult>> _func;

        private readonly Dictionary<TSource, (IReactiveSubscription subscription, int count)> _subscriptions
            = new Dictionary<TSource, (IReactiveSubscription subscription, int count)>(EqualityComparerFactory
                .GetEqualityComparer<TSource>());

        public SelectManyObservableOperator(IReactiveCollection<TSource> source,
            Func<TSource, ReactiveCollectorToken, IReactiveCollection<TResult>> func) : base(source)
        {
            _func = func;
        }

        protected override void UnfollowAll()
        {
            foreach (var subscription in _subscriptions.Values) subscription.subscription.Dispose();
        }

        protected override void Unfollow(TSource source)
        {
            if (_subscriptions.TryGetValue(source, out var tuple))
            {
                if (tuple.count == 1)
                {
                    tuple.subscription.Dispose();
                    _subscriptions.Remove(source);
                }
                else
                {
                    _subscriptions[source] = (tuple.subscription, tuple.count - 1);
                }
            }
        }

        protected override void Follow(TSource source)
        {
            if (_subscriptions.TryGetValue(source, out var tuple))
                _subscriptions[source] = (tuple.subscription, tuple.count + 1);
            else
                _subscriptions[source] = (_func(source, CollectorToken).AddObserver(this), tuple.count + 1);
        }

        protected override IEnumerable<TResult> GetItems(TSource arg)
        {
            return _func(arg, CollectorToken).GetValue(CollectorToken);
        }
    }
}