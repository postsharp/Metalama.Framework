#region

using System;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace Caravela.Reactive
{
    internal abstract class SelectManyOperator<TSource, TCollection, TResult> : ReactiveCollectionOperator<TSource, TResult>,
        IReactiveCollectionObserver<TCollection>
    {
        private IEnumerable<TResult> _results = null!;
        protected Func<TSource, TCollection, ReactiveCollectorToken, TResult> ResultSelector { get; }

        protected SelectManyOperator(IReactiveCollection<TSource> source, Func<TSource, TCollection, ReactiveCollectorToken, TResult> resultSelector) : base(source)
        {
            ResultSelector = resultSelector;
        }

        protected abstract TResult SelectResult(IReactiveSubscription subscription, TCollection item);

        void IReactiveCollectionObserver<TCollection>.OnItemAdded(IReactiveSubscription subscription, TCollection item,
            int newVersion)
        {
            if (!this.CanProcessIncrementalChange)
                return;
            
            using var updateToken = this.GetIncrementalUpdateToken();

            AddItem(SelectResult(subscription, item), updateToken);
        }

        void IReactiveCollectionObserver<TCollection>.OnItemRemoved(IReactiveSubscription subscription, TCollection item,
            int newVersion)
        {
            if (!this.CanProcessIncrementalChange)
                return;
            
            using var updateToken = this.GetIncrementalUpdateToken();

            RemoveItem(SelectResult(subscription, item), updateToken);
        }

        void IReactiveCollectionObserver<TCollection>.OnItemReplaced(IReactiveSubscription subscription, TCollection oldItem,
            TCollection newItem, int newVersion)
        {
            if (!this.CanProcessIncrementalChange)
                return;
            
            using var updateToken = this.GetIncrementalUpdateToken();

            RemoveItem(SelectResult(subscription, oldItem), updateToken);
            AddItem(SelectResult(subscription, newItem), updateToken);
        }

        void IReactiveObserver.OnValueInvalidated(IReactiveSubscription subscription,
            bool isBreakingChange)
        {
            if (isBreakingChange)
            {
                this.OnBreakingChange();
            }
        }

        void IReactiveObserver<IEnumerable<TCollection>>.OnValueChanged(IReactiveSubscription subscription,
            IEnumerable<TCollection> oldValue, IEnumerable<TCollection> newValue, int newVersion,
            bool isBreakingChange)
        {
            if (isBreakingChange)
            {
                this.OnBreakingChange();
            }
        }


        protected abstract void UnfollowAll();
        protected abstract void Unfollow(TSource source);
        protected abstract void Follow(TSource source);

        protected abstract IEnumerable<TResult> GetItems(TSource arg);


        protected override bool EvaluateFunction(IEnumerable<TSource> source)
        {
            this.UnfollowAll();

            foreach (var s in source)
            {
                this.Follow(s);
            }

            this._results = source.SelectMany(this.GetItems);

            return true;
        }


        private void AddItem(TResult addedItem, in UpdateToken updateToken)
        {
            updateToken.SignalChange();

            // We have a new item.

            foreach (var observer in this.Observers)
            {
                observer.Observer.OnItemAdded(observer.Subscription, addedItem, updateToken.Version);
            }
        }

        private void RemoveItem(TResult removedItem, in UpdateToken updateToken)
        {
            updateToken.SignalChange();


            foreach (var observer in this.Observers)
            {
                observer.Observer.OnItemRemoved(observer.Subscription, removedItem, updateToken.Version);
            }
        }

        private void AddSource(TSource source, in UpdateToken updateToken)
        {
            this.Follow(source);


            foreach (var newItem in this.GetItems(source))
            {
                this.AddItem(newItem, updateToken);
            }
        }

        private void RemoveSource(TSource source, in UpdateToken updateToken)
        {
            this.Unfollow(source);

            foreach (var removedItem in this.GetItems(source))
            {
                this.RemoveItem(removedItem, updateToken);
            }
        }


        protected override void OnSourceItemAdded(IReactiveSubscription sourceSubscription, TSource item,
            in UpdateToken updateToken)
        {
            this.AddSource(item, updateToken);
        }

        protected override void OnSourceItemRemoved(IReactiveSubscription sourceSubscription, TSource item,
            in UpdateToken updateToken)
        {
            this.RemoveSource(item, updateToken);
        }

        protected override void OnSourceItemReplaced(IReactiveSubscription sourceSubscription, TSource oldItem,
            TSource newItem, in UpdateToken updateToken)
        {
            this.RemoveSource(oldItem, updateToken);
            this.AddSource(newItem, updateToken);
        }


        protected override IEnumerable<TResult> GetFunctionResult()
        {
            return this._results;
        }
    }
}