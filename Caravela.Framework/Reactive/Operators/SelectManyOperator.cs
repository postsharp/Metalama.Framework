#region

using System;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace Caravela.Reactive
{
    internal abstract class SelectManyOperator<TSource, TResult> : ReactiveCollectionOperator<TSource, TResult>,
        IReactiveCollectionObserver<TResult>
    {

        protected SelectManyOperator(IReactiveCollection<TSource> source) : base(source)
        {
        }


        void IReactiveCollectionObserver<TResult>.OnItemAdded(IReactiveSubscription subscription, TResult item,
            int newVersion)
        {
            if (!this.ShouldProcessIncrementalChange)
                return;
            
            using var updateToken = this.GetIncrementalUpdateToken();

            this.AddItem(item, updateToken);
        }

        void IReactiveCollectionObserver<TResult>.OnItemRemoved(IReactiveSubscription subscription, TResult item,
            int newVersion)
        {
            if (!this.ShouldProcessIncrementalChange)
                return;
            
            using var updateToken = this.GetIncrementalUpdateToken(newVersion);

            this.RemoveItem(item, updateToken);
        }

        void IReactiveCollectionObserver<TResult>.OnItemReplaced(IReactiveSubscription subscription, TResult oldItem,
            TResult newItem, int newVersion)
        {
            if (!this.ShouldProcessIncrementalChange)
                return;
            
            using var updateToken = this.GetIncrementalUpdateToken(newVersion);

            this.RemoveItem(oldItem, updateToken);
            this.AddItem(newItem, updateToken);
        }

        void IReactiveObserver.OnValueInvalidated(IReactiveSubscription subscription,
            bool isBreakingChange)
        {
            if (isBreakingChange)
            {
                this.OnObserverBreakingChange();
            }
        }

        void IReactiveObserver<IEnumerable<TResult>>.OnValueChanged(IReactiveSubscription subscription,
            IEnumerable<TResult> oldValue, IEnumerable<TResult> newValue, int newVersion,
            bool isBreakingChange)
        {
            if (isBreakingChange)
            {
                this.OnObserverBreakingChange();
            }
        }


        protected abstract void UnfollowAll();
        protected abstract void Unfollow(TSource source);
        protected abstract void Follow(TSource source);

        protected abstract IEnumerable<TResult> GetItems(TSource arg);


        protected override IEnumerable<TResult> EvaluateFunction(IEnumerable<TSource> source)
        {
            this.UnfollowAll();

            foreach (var s in source)
            {
                this.Follow(s);
            }

            return source.SelectMany(this.GetItems);
        }

        private void AddItem(TResult addedItem, in IncrementalUpdateToken updateToken)
        {
            updateToken.SignalChange(true);

            // We have a new item.

            foreach (var observer in this.Observers)
            {
                observer.Observer.OnItemAdded(observer.Subscription, addedItem, updateToken.NextVersion);
            }
        }

        private void RemoveItem(TResult removedItem, in IncrementalUpdateToken updateToken)
        {
            updateToken.SignalChange(true);


            foreach (var observer in this.Observers)
            {
                observer.Observer.OnItemRemoved(observer.Subscription, removedItem, updateToken.NextVersion);
            }
        }

        private void AddSource(TSource source, in IncrementalUpdateToken updateToken)
        {
            this.Follow(source);


            foreach (var newItem in this.GetItems(source))
            {
                this.AddItem(newItem, updateToken);
            }
        }

        private void RemoveSource(TSource source, in IncrementalUpdateToken updateToken)
        {
            this.Unfollow(source);

            foreach (var removedItem in this.GetItems(source))
            {
                this.RemoveItem(removedItem, updateToken);
            }
        }


        protected override void OnSourceItemAdded(IReactiveSubscription sourceSubscription, TSource item,
            in IncrementalUpdateToken updateToken)
        {
            this.AddSource(item, updateToken);
        }

        protected override void OnSourceItemRemoved(IReactiveSubscription sourceSubscription, TSource item,
            in IncrementalUpdateToken updateToken)
        {
            this.RemoveSource(item, updateToken);
        }

        protected override void OnSourceItemReplaced(IReactiveSubscription sourceSubscription, TSource oldItem,
            TSource newItem, in IncrementalUpdateToken updateToken)
        {
            this.RemoveSource(oldItem, updateToken);
            this.AddSource(newItem, updateToken);
        }


       
    }
}