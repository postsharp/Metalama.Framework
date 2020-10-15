using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Caravela.Reactive
{
    internal class GroupByOperator<TKey, TItem> : ReactiveCollectionOperator<TItem, Group<TKey, TItem>>,
        IGroupBy<TKey, TItem>
    {
        private readonly IEqualityComparer<TKey> _equalityComparer;
        private readonly Func<TItem, ReactiveCollectorToken, TKey> _getKeyFunc;
        private ImmutableDictionary<TKey, Group<TKey, TItem>> _groups;

        public GroupByOperator(IReactiveCollection<TItem> source, Func<TItem, ReactiveCollectorToken, TKey> getKeyFunc,
            IEqualityComparer<TKey>? equalityComparer) : base(source)
        {
            _equalityComparer = equalityComparer ?? EqualityComparer<TKey>.Default;
            _getKeyFunc = getKeyFunc;
            _groups = ImmutableDictionary<TKey, Group<TKey, TItem>>.Empty.WithComparers(_equalityComparer);
        }

        public Group<TKey, TItem> this[TKey key]
        {
            get
            {
                EnsureFunctionEvaluated();

                if (!_groups.TryGetValue(key, out var group))
                {
                    using var token = GetUpdateToken();

                    if (!_groups.TryGetValue(key, out group))
                    {
                        token.SignalChange();

                        // We never return a null group, instead we create an empty group to which
                        // items can be added later.
                        group = new Group<TKey, TItem>(this, key);

                        var oldGroups = _groups;
                        _groups = _groups.Add(key, group);


                        if (HasObserver)
                            foreach (var subscription in Observers)
                            {
                                subscription.Observer.OnItemAdded(subscription, @group, token.Version);
                                subscription.Observer.OnValueChanged(subscription, oldGroups.Values, _groups.Values,
                                    token.Version);
                            }
                    }
                }

                return group;
            }
        }


        private void AddItem(ref ImmutableDictionary<TKey, Group<TKey, TItem>> newResult, TItem item,
            in UpdateToken updateToken)
        {
            var key = _getKeyFunc(item, CollectorToken);
            if (!newResult.TryGetValue(key, out var group))
            {
                group = new Group<TKey, TItem>(this, key);

                newResult = newResult.Add(key, group);

                updateToken.SignalChange();

                foreach (var subscription in Observers)
                    subscription.Observer.OnItemAdded(subscription, @group, updateToken.Version);
            }

            group.Add(item);
        }


        private void RemoveItem(ref ImmutableDictionary<TKey, Group<TKey, TItem>> newResult, TItem removedItem,
            in UpdateToken updateToken)
        {
            var key = _getKeyFunc(removedItem, CollectorToken);
            if (newResult.TryGetValue(key, out var group))
            {
                group.Remove(removedItem);

                if (group.Count == 0 && !group.HasObserver)
                {
                    newResult = newResult.Remove(key);

                    updateToken.SignalChange();

                    foreach (var subscription in Observers)
                        subscription.Observer.OnItemRemoved(subscription, @group, updateToken.Version);
                }
            }
        }


        protected override bool EvaluateFunction(IEnumerable<TItem> source)
        {
            _groups =
                source.GroupBy(s => _getKeyFunc(s, CollectorToken)).ToImmutableDictionary(g => g.Key,
                    g => new Group<TKey, TItem>(this, g));

            return true;
        }

        protected override IEnumerable<Group<TKey, TItem>> GetFunctionResult()
        {
            return _groups.Values;
        }

        protected override void OnSourceItemAdded(IReactiveSubscription sourceSubscription, TItem item,
            in UpdateToken updateToken)
        {
            var newResult = _groups;

            AddItem(ref newResult, item, in updateToken);
        }

        protected override void OnSourceItemRemoved(IReactiveSubscription sourceSubscription, TItem item,
            in UpdateToken updateToken)
        {
            var newResult = _groups;

            RemoveItem(ref newResult, item, in updateToken);
        }

        protected override void OnSourceItemReplaced(IReactiveSubscription sourceSubscription, TItem oldItem,
            TItem newItem, in UpdateToken updateToken)
        {
            var newResult = _groups;

            RemoveItem(ref newResult, oldItem, in updateToken);
            AddItem(ref newResult, newItem, in updateToken);
        }

        public override bool IsMaterialized => true;
    }
}