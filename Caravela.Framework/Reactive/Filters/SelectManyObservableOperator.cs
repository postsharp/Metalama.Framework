using System;
using System.Collections.Generic;

namespace Caravela.Reactive
{
    internal abstract class SelectManyObservableOperatorBase<TSource, TCollection, TResult> : SelectManyOperator<TSource, TCollection, TResult>
    {
        protected Func<TSource, ReactiveCollectorToken, IReactiveCollection<TCollection>> CollectionSelector { get; }

        private readonly Dictionary<TSource, (IReactiveSubscription subscription, int count)> _subscriptions
            = new Dictionary<TSource, (IReactiveSubscription subscription, int count)>(EqualityComparerFactory
                .GetEqualityComparer<TSource>());

        public SelectManyObservableOperatorBase(IReactiveCollection<TSource> source,
            Func<TSource, ReactiveCollectorToken, IReactiveCollection<TCollection>> collectionSelector,
            Func<TSource, TCollection, ReactiveCollectorToken, TResult> resultSelector) : base(source, resultSelector)
        {
            CollectionSelector = collectionSelector;
        }

        protected override void UnfollowAll()
        {
            foreach (var subscription in _subscriptions.Values)
                subscription.subscription.Dispose();
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
                _subscriptions[source] = (CollectionSelector(source, CollectorToken).AddObserver(this), tuple.count + 1);
        }
    }

    internal sealed class SelectManyObservableOperator<TSource, TResult> : SelectManyObservableOperatorBase<TSource, TResult, TResult>
    {
        public SelectManyObservableOperator(
            IReactiveCollection<TSource> source, Func<TSource, ReactiveCollectorToken, IReactiveCollection<TResult>> collectionSelector)
            : base(source, collectionSelector, (source, result, token) => result)
        {
        }

        protected override IEnumerable<TResult> GetItems(TSource arg)
        {
            return CollectionSelector(arg, CollectorToken).GetValue(CollectorToken);
        }
    }

    internal sealed class SelectManyObservableOperator<TSource, TCollection, TResult> : SelectManyObservableOperatorBase<TSource, TCollection, TResult>
    {
        public SelectManyObservableOperator(
            IReactiveCollection<TSource> source,
            Func<TSource, ReactiveCollectorToken, IReactiveCollection<TCollection>> collectionSelector,
            Func<TSource, TCollection, ReactiveCollectorToken, TResult> resultSelector)
            : base(source, collectionSelector, resultSelector)
        {
        }

        protected override IEnumerable<TResult> GetItems(TSource arg)
        {
            foreach (var item in CollectionSelector(arg, CollectorToken).GetValue(CollectorToken))
            {
                yield return ResultSelector(arg, item, CollectorToken);
            }
        }
    }
}