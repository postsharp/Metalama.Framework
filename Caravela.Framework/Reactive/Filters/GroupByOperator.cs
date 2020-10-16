#region

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

#endregion

namespace Caravela.Reactive
{
    internal class GroupByOperator<TKey, TItem> : ReactiveCollectionOperator<TItem, Group<TKey, TItem>>,
        IGroupBy<TKey, TItem>
    {
        private readonly IEqualityComparer<TKey> _equalityComparer;
        private readonly Func<TItem, ReactiveCollectorToken, TKey> _getKeyFunc;
        private ImmutableDictionary<TKey, Group<TKey, TItem>> _groups;

        public GroupByOperator(IReactiveCollection<TItem> source, Func<TItem, ReactiveCollectorToken, TKey> getKeyFunc,
            IEqualityComparer<TKey> equalityComparer) : base(source)
        {
            this._equalityComparer = equalityComparer ?? EqualityComparer<TKey>.Default;
            this._getKeyFunc = getKeyFunc;
            this._groups = ImmutableDictionary<TKey, Group<TKey, TItem>>.Empty.WithComparers(this._equalityComparer);
        }

        public Group<TKey, TItem> this[TKey key]
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
                        // items can be added later. This is important because the consumer may add an observer.
                        group = new Group<TKey, TItem>(this, key);

                        var oldGroups = this._groups;
                        this._groups = this._groups.Add(key, group);

                        foreach (var subscription in this.Observers)
                        {
                            subscription.Observer.OnItemAdded(subscription.Subscription, @group, token.Version);
                        }

                        foreach (var subscription in this.Observers.OfType<IEnumerable<Group<TKey,TItem>>>())
                        {
                            subscription.Observer.OnValueChanged(subscription.Subscription, oldGroups.Values, this._groups.Values, token.Version);
                        }
                    }
                }

                return group;
            }
        }

        public override bool IsMaterialized => true;


        private void AddItem(ref ImmutableDictionary<TKey, Group<TKey, TItem>> newResult, TItem item,
            in UpdateToken updateToken)
        {
            var key = this._getKeyFunc(item, this.CollectorToken);
            if (!newResult.TryGetValue(key, out var group))
            {
                group = new Group<TKey, TItem>(this, key);

                newResult = newResult.Add(key, group);

                updateToken.SignalChange();

                foreach (var subscription in this.Observers)
                {
                    subscription.Observer.OnItemAdded(subscription.Subscription, @group, updateToken.Version);
                }
            }

            group.Add(item);
        }


        private void RemoveItem(ref ImmutableDictionary<TKey, Group<TKey, TItem>> newResult, TItem removedItem,
            in UpdateToken updateToken)
        {
            var key = this._getKeyFunc(removedItem, this.CollectorToken);
            if (newResult.TryGetValue(key, out var group))
            {
                group.Remove(removedItem);

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


        protected override bool EvaluateFunction(IEnumerable<TItem> source)
        {
            // We cannot simply overwrite the dictionary with a brand new one because there may be observers on individual
            // groups and we need to preserve them.

            var oldGroups = this._groups;

            var builder = oldGroups.ToBuilder();

            foreach (var group in source.GroupBy(s => this._getKeyFunc(s, this.CollectorToken)))
            {
                if (builder.TryGetValue(group.Key, out var items))
                {
                    items.Replace(group, this.Version);
                }
            }

            foreach (var group in builder)
            {
                if (group.Value.Mark != this.Version)
                {
                    if (group.Value.HasObserver)
                    {
                        group.Value.Clear();
                    }
                    else
                    {
                        builder.Remove(group.Key);
                    }
                }
            }

            this._groups = builder.ToImmutable();
                
            return true;
        }

        protected override IEnumerable<Group<TKey, TItem>> GetFunctionResult()
        {
            return this._groups.Values;
        }

        protected override void OnSourceItemAdded(IReactiveSubscription sourceSubscription, TItem item,
            in UpdateToken updateToken)
        {
            var newResult = this._groups;

            this.AddItem(ref newResult, item, in updateToken);

            this._groups = newResult;
        }

        protected override void OnSourceItemRemoved(IReactiveSubscription sourceSubscription, TItem item,
            in UpdateToken updateToken)
        {
            var newResult = this._groups;
            
            this.RemoveItem(ref newResult, item, in updateToken);
            this._groups = newResult;
        }

        protected override void OnSourceItemReplaced(IReactiveSubscription sourceSubscription, TItem oldItem,
            TItem newItem, in UpdateToken updateToken)
        {
            var newResult = this._groups;

            this.RemoveItem(ref newResult, oldItem, in updateToken);
            this.AddItem(ref newResult, newItem, in updateToken);
            this._groups = newResult;
        }
    }
}