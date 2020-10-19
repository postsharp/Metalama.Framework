#region

using System;
using System.Collections.Generic;

#endregion

namespace Caravela.Reactive
{
    internal class SelectManyObservableOperator<TSource, TResult> : SelectManyOperator<TSource, TResult>
    {
        private readonly Func<TSource, ReactiveObserverToken, IReactiveCollection<TResult>> _func;

        private readonly Dictionary<TSource, (IReactiveSubscription subscription, int count)> _subscriptions
            = new Dictionary<TSource, (IReactiveSubscription subscription, int count)>(EqualityComparerFactory
                .GetEqualityComparer<TSource>());

        public SelectManyObservableOperator(IReactiveCollection<TSource> source,
            Func<TSource, IReactiveCollection<TResult>> func) : base(source)
        {
            this._func = ReactiveObserverToken.WrapWithDefaultToken(func);
        }

        protected override void UnfollowAll()
        {
            foreach (var subscription in this._subscriptions.Values)
            {
                subscription.subscription.Dispose();
            }
        }

        protected override void Unfollow(TSource source)
        {
            if (this._subscriptions.TryGetValue(source, out var tuple))
            {
                if (tuple.count == 1)
                {
                    tuple.subscription.Dispose();
                    this._subscriptions.Remove(source);
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
                this._subscriptions[source] =
                    (this._func(source, this.ObserverToken).AddObserver(this), tuple.count + 1);
            }
        }

        protected override IEnumerable<TResult> GetItems(TSource arg)
        {
            return this._func(arg, this.ObserverToken).GetValue(this.ObserverToken);
        }
    }
}