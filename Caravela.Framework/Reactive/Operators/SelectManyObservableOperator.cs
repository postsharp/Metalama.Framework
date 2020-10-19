#region

using System;
using System.Collections.Generic;

#endregion

namespace Caravela.Reactive
{
    internal abstract class SelectManyObservableOperatorBase<TSource, TCollection, TResult> : SelectManyOperator<TSource, TCollection, TResult>
    {
        protected Func<TSource, ReactiveObserverToken, IReactiveCollection<TCollection>> CollectionSelector { get; }

        private readonly Dictionary<TSource, (IReactiveSubscription? subscription, int count)> _subscriptions
            = new(EqualityComparerFactory.GetEqualityComparer<TSource>());
        private readonly Dictionary<IReactiveSubscription, TSource> _subscriptionsReverse = new();

        public SelectManyObservableOperatorBase(IReactiveCollection<TSource> source,
            Func<TSource, IReactiveCollection<TCollection>> collectionSelector,
            Func<TSource, TCollection, TResult> resultSelector) : base(source, resultSelector)
        {
            CollectionSelector = ReactiveObserverToken.WrapWithDefaultToken(collectionSelector);
        }

        protected override TResult SelectResult(IReactiveSubscription subscription, TCollection item) =>
            ResultSelector(_subscriptionsReverse[subscription], item, this.ObserverToken);

        protected override void UnfollowAll()
        {
            foreach (var subscription in this._subscriptions.Values)
            {
                subscription.subscription?.Dispose();
            }
            this._subscriptionsReverse.Clear();
        }

        protected override void Unfollow(TSource source)
        {
            if (this._subscriptions.TryGetValue(source, out var tuple))
            {
                if (tuple.count == 1)
                {
                    tuple.subscription?.Dispose();
                    this._subscriptions.Remove(source);
                    if (tuple.subscription != null)
                        this._subscriptionsReverse.Remove(tuple.subscription);
                }
                else
                {
                    this._subscriptions[source] = (tuple.subscription, tuple.count - 1);
                }
            }
        }

        protected override void Follow(TSource source)
        {
            if (this._subscriptions.TryGetValue(source, out var tuple))
            {
                this._subscriptions[source] = (tuple.subscription, tuple.count + 1);
            }
            else
            {
                var subscription = CollectionSelector(source, this.ObserverToken).AddObserver(this);
                _subscriptions.Add(source, (subscription, 1));
                if (subscription != null)
                    _subscriptionsReverse.Add(subscription, source);
            }
        }
    }

    internal sealed class SelectManyObservableOperator<TSource, TResult> : SelectManyObservableOperatorBase<TSource, TResult, TResult>
    {
        public SelectManyObservableOperator(
            IReactiveCollection<TSource> source, Func<TSource, IReactiveCollection<TResult>> collectionSelector)
            : base(source, collectionSelector, (source, result) => result)
        {
        }

        protected override IEnumerable<TResult> GetItems(TSource arg)
        {
            return CollectionSelector(arg, this.ObserverToken).GetValue(this.ObserverToken);
        }
    }

    internal sealed class SelectManyObservableOperator<TSource, TCollection, TResult> : SelectManyObservableOperatorBase<TSource, TCollection, TResult>
    {
        public SelectManyObservableOperator(
            IReactiveCollection<TSource> source,
            Func<TSource, IReactiveCollection<TCollection>> collectionSelector,
            Func<TSource, TCollection, TResult> resultSelector)
            : base(source, collectionSelector, resultSelector)
        {
        }

        protected override IEnumerable<TResult> GetItems(TSource arg)
        {
            foreach (var item in CollectionSelector(arg, this.ObserverToken).GetValue(this.ObserverToken))
            {
                yield return ResultSelector(arg, item, this.ObserverToken);
            }
        }
    }
}