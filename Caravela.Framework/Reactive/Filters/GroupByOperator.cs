#region

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

#endregion

namespace Caravela.Reactive
{
    internal class GroupByOperator<TSource, TKey, TElement> : ReactiveCollectionOperator<TSource, Group<TKey, TElement>>,
        IGroupBy<TKey, TElement>
    {
        private readonly IEqualityComparer<TKey> _equalityComparer;
        private readonly Func<TSource, ReactiveCollectorToken, TKey> _getKeyFunc;
        private readonly Func<TSource, TElement> _getElementFunc;
        private ImmutableDictionary<TKey, Group<TKey, TElement>> _groups;

        public GroupByOperator(
            IReactiveCollection<TSource> source,
            Func<TSource, ReactiveCollectorToken, TKey> getKeyFunc,
            Func<TSource, TElement> getElementFunc,
            IEqualityComparer<TKey>? equalityComparer) : base(source)
        {
            this._equalityComparer = equalityComparer ?? EqualityComparer<TKey>.Default;
            this._getKeyFunc = getKeyFunc;
            this._getElementFunc = getElementFunc;
            this._groups = ImmutableDictionary<TKey, Group<TKey, TElement>>.Empty.WithComparers(this._equalityComparer);
        }

        public Group<TKey, TElement> this[TKey key]
        {
            get
            {
                this.EnsureFunctionEvaluated();

                if (!this._groups.TryGetValue(key, out var group))
                {
                    using var token = this.GetIncrementalUpdateToken();

                    if (!this._groups.TryGetValue(key, out group))
                    {
                        token.SignalChange();

                        // We never return a null group, instead we create an empty group to which
                        // items can be added later.
                        group = new Group<TKey, TElement>(this, key);

                        var oldGroups = this._groups;
                        this._groups = this._groups.Add(key, group);

                        foreach (var subscription in this.Observers)
                        {
                            subscription.Observer.OnItemAdded(subscription.Subscription, group, token.Version);
                            
                        }

                        foreach (var subscription in this.Observers.OfType<IEnumerable<Group<TKey, TElement>>>())
                        {
                            subscription.Observer.OnValueChanged(subscription.Subscription, oldGroups.Values, this._groups.Values, token.Version);
                        }
                    }
                }

                return group;
            }
        }

        public override bool IsMaterialized => true;


        private void AddItem(ref ImmutableDictionary<TKey, Group<TKey, TElement>> newResult, TSource item,
            in UpdateToken updateToken)
        {
            var key = this._getKeyFunc(item, this.CollectorToken);
            if (!newResult.TryGetValue(key, out var group))
            {
                group = new Group<TKey, TElement>(this, key);

                newResult = newResult.Add(key, group);

                updateToken.SignalChange();

                foreach (var subscription in this.Observers)
                {
                    subscription.Observer.OnItemAdded(subscription.Subscription, group, updateToken.Version);
                }
            }

            var element = _getElementFunc(item);

            group.Add(element);
        }


        private void RemoveItem(ref ImmutableDictionary<TKey, Group<TKey, TElement>> newResult, TSource removedItem,
            in UpdateToken updateToken)
        {
            var key = this._getKeyFunc(removedItem, this.CollectorToken);
            if (newResult.TryGetValue(key, out var group))
            {
                var element = _getElementFunc(removedItem);

                group.Remove(element);

                if (group.Count == 0)
                {
                    if (!group.HasObserver)
                    {
                        newResult = newResult.Remove(key);
                    }
                    else
                    {
                        // We can't remove a group that has observers because they would not get
                        // notified the next time we're adding something to a group of the same key.
                    }

                    updateToken.SignalChange();

                    foreach (var subscription in this.Observers)
                    {
                        subscription.Observer.OnItemRemoved(subscription.Subscription, @group, updateToken.Version);
                    }
                }
            }
        }


        protected override bool EvaluateFunction(IEnumerable<TSource> source)
        {
            this._groups = source
                .GroupBy(s => this._getKeyFunc(s, this.CollectorToken), _getElementFunc)
                .ToImmutableDictionary(g => g.Key, g => new Group<TKey, TElement>(this, g));

            return true;
        }

        protected override IEnumerable<Group<TKey, TElement>> GetFunctionResult()
        {
            return this._groups.Values;
        }

        protected override void OnSourceItemAdded(IReactiveSubscription sourceSubscription, TSource item,
            in UpdateToken updateToken)
        {
            var newResult = this._groups;

            this.AddItem(ref newResult, item, in updateToken);

            this._groups = newResult;
        }

        protected override void OnSourceItemRemoved(IReactiveSubscription sourceSubscription, TSource item,
            in UpdateToken updateToken)
        {
            var newResult = this._groups;
            this.RemoveItem(ref newResult, item, in updateToken);
            this._groups = newResult;
        }

        protected override void OnSourceItemReplaced(IReactiveSubscription sourceSubscription, TSource oldItem,
            TSource newItem, in UpdateToken updateToken)
        {
            var newResult = this._groups;

            this.RemoveItem(ref newResult, oldItem, in updateToken);
            this.AddItem(ref newResult, newItem, in updateToken);
            this._groups = newResult;
        }
    }
}