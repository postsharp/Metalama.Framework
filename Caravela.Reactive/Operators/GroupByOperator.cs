#region

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

#endregion

namespace Caravela.Reactive
{
    internal class GroupByOperator<TSource, TKey, TElement> : ReactiveCollectionOperator<TSource, IReactiveGroup<TKey, TElement>>,
        IReactiveGroupBy<TKey, TElement>, IGroupByOperator<TKey, TElement>
    {
        private readonly IEqualityComparer<TKey> _equalityComparer;
        private readonly Func<TSource, ReactiveObserverToken, TKey> _getKeyFunc;
        private readonly Func<TSource, TElement> _getElementFunc;
        private ImmutableDictionary<TKey, Group<TKey, TElement>> _groups;

        public GroupByOperator(
            IReactiveCollection<TSource> source,
            Func<TSource, TKey> getKeyFunc,
            Func<TSource, TElement> getElementFunc,
            IEqualityComparer<TKey>? equalityComparer) : base(source)
        {
            this._equalityComparer = equalityComparer ?? EqualityComparer<TKey>.Default;
            this._getKeyFunc = ReactiveObserverToken.WrapWithDefaultToken(getKeyFunc);
            this._getElementFunc = getElementFunc;
            this._groups = ImmutableDictionary<TKey, Group<TKey, TElement>>.Empty.WithComparers(this._equalityComparer);
        }

        public IReactiveGroup<TKey, TElement> this[TKey key]
        {
            get
            {
                this.EnsureFunctionEvaluated();

                if (!this._groups.TryGetValue(key, out var group))
                {
                    using var token = this.GetIncrementalUpdateToken(-1);

                    if (!this._groups.TryGetValue(key, out group))
                    {
                        token.SignalChange();

                        // We never return a null group, instead we create an empty group to which
                        // items can be added later. This is important because the consumer may add an observer.
                        group = new Group<TKey, TElement>(this, key);

                        var oldGroups = this._groups;
                        this._groups = this._groups.Add(key, group);

                        foreach (var subscription in this.Observers)
                        {
                            subscription.Observer.OnItemAdded(subscription.Subscription, group, token.NextVersion);
                        }

                        foreach (var subscription in this.Observers.OfType<IEnumerable<Group<TKey, TElement>>>())
                        {
                            subscription.Observer.OnValueChanged(subscription.Subscription, oldGroups.Values, this._groups.Values, token.NextVersion);
                        }
                    }
                }

                return group;
            }
        }

        public override bool IsMaterialized => true;

        void IGroupByOperator<TKey, TElement>.EnsureSubscribedToSource() => this.EnsureSubscribedToSource();

        private void AddItem(ref ImmutableDictionary<TKey, Group<TKey, TElement>> newResult, TSource item,
            in IncrementalUpdateToken updateToken)
        {
            var key = this._getKeyFunc(item, this.ObserverToken);
            if (!newResult.TryGetValue(key, out var group))
            {
                group = new Group<TKey, TElement>(this, key);

                newResult = newResult.Add(key, group);

                updateToken.SignalChange();

                foreach (var subscription in this.Observers)
                {
                    subscription.Observer.OnItemAdded(subscription.Subscription, group, updateToken.NextVersion);
                }
            }

            var element = this._getElementFunc(item);

            group.Add(element);
        }


        private void RemoveItem(ref ImmutableDictionary<TKey, Group<TKey, TElement>> newResult, TSource removedItem,
            in IncrementalUpdateToken updateToken)
        {
            var key = this._getKeyFunc(removedItem, this.ObserverToken);
            if (newResult.TryGetValue(key, out var group))
            {
                var element = this._getElementFunc(removedItem);

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
                        subscription.Observer.OnItemRemoved(subscription.Subscription, @group, updateToken.NextVersion);
                    }
                }
            }
        }


        protected override IEnumerable<IReactiveGroup<TKey, TElement>> EvaluateFunction(IEnumerable<TSource> source)
        {
            // We cannot simply overwrite the dictionary with a brand new one because there may be observers on individual
            // groups and we need to preserve them.

            var oldGroups = this._groups;

            var builder = oldGroups.ToBuilder();

            foreach (var group in source.GroupBy(s => this._getKeyFunc(s, this.ObserverToken), this._getElementFunc ))
            {
                if (builder.TryGetValue(group.Key, out var items))
                {
                    items.Replace(group, this.Version);
                }
                else
                {
                    builder.Add( group.Key, new Group<TKey, TElement>( this, group, this.Version ) );
                }
            }

            foreach (var group in builder.ToImmutable())
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
                
            return this._groups.Values;
        }

        protected override void OnSourceItemAdded(IReactiveSubscription sourceSubscription, TSource item,
            in IncrementalUpdateToken updateToken)
        {
            var newResult = this._groups;

            this.AddItem(ref newResult, item, in updateToken);

            this._groups = newResult;
            
            updateToken.SetNewValue(this._groups.Values);
        }

        protected override void OnSourceItemRemoved(IReactiveSubscription sourceSubscription, TSource item,
            in IncrementalUpdateToken updateToken)
        {
            var newResult = this._groups;
            
            this.RemoveItem(ref newResult, item, in updateToken);
            
            this._groups = newResult;
            
            updateToken.SetNewValue(this._groups.Values);
        }

        protected override void OnSourceItemReplaced(IReactiveSubscription sourceSubscription, TSource oldItem,
            TSource newItem, in IncrementalUpdateToken updateToken)
        {
            var newResult = this._groups;

            this.RemoveItem(ref newResult, oldItem, in updateToken);
            this.AddItem(ref newResult, newItem, in updateToken);
            this._groups = newResult;
            
            updateToken.SetNewValue(this._groups.Values);
        }
    }
}