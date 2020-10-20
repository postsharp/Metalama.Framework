#region

using System;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace Caravela.Reactive.Operators
{
    internal abstract class SelectManyOperator<TSource, TCollection, TResult> : ReactiveCollectionOperator<TSource, TResult>,
        IReactiveCollectionObserver<TCollection>
    {
        protected Func<TSource, TCollection, ReactiveObserverToken, TResult> ResultSelector { get; }

        protected SelectManyOperator(IReactiveCollection<TSource> source, Func<TSource, TCollection, TResult> resultSelector) : base(source)
        {
            this.ResultSelector = ReactiveObserverToken.WrapWithDefaultToken(resultSelector);
        }

        protected abstract TResult SelectResult(IReactiveSubscription subscription, TCollection item);

        void IReactiveCollectionObserver<TCollection>.OnItemAdded(IReactiveSubscription subscription, TCollection item,
            int newVersion)
        {
            if (!this.ShouldProcessIncrementalChange)
                return;
            
            using var updateToken = this.GetIncrementalUpdateToken();

<<<<<<< HEAD
            this.AddItem( this.SelectResult(subscription, item), updateToken);
=======
            this.AddItem(this.SelectResult(subscription, item), updateToken);
>>>>>>> Reactive: move to namespaces and make more things public
        }

        void IReactiveCollectionObserver<TCollection>.OnItemRemoved(IReactiveSubscription subscription, TCollection item,
            int newVersion)
        {
            if (!this.ShouldProcessIncrementalChange)
                return;
            
            using var updateToken = this.GetIncrementalUpdateToken(newVersion);

<<<<<<< HEAD
            this.RemoveItem( this.SelectResult(subscription, item), updateToken);
=======
            this.RemoveItem(this.SelectResult(subscription, item), updateToken);
>>>>>>> Reactive: move to namespaces and make more things public
        }

        void IReactiveCollectionObserver<TCollection>.OnItemReplaced(IReactiveSubscription subscription, TCollection oldItem,
            TCollection newItem, int newVersion)
        {
            if (!this.ShouldProcessIncrementalChange)
                return;
            
            using var updateToken = this.GetIncrementalUpdateToken(newVersion);

<<<<<<< HEAD
            this.RemoveItem( this.SelectResult(subscription, oldItem), updateToken);
            this.AddItem( this.SelectResult(subscription, newItem), updateToken);
=======
            this.RemoveItem(this.SelectResult(subscription, oldItem), updateToken);
            this.AddItem(this.SelectResult(subscription, newItem), updateToken);
>>>>>>> Reactive: move to namespaces and make more things public
        }

        void IReactiveObserver.OnValueInvalidated(IReactiveSubscription subscription,
            bool isBreakingChange)
        {
            if (isBreakingChange)
            {
                this.OnObserverBreakingChange();
            }
        }

        void IReactiveObserver<IEnumerable<TCollection>>.OnValueChanged(IReactiveSubscription subscription,
            IEnumerable<TCollection> oldValue, IEnumerable<TCollection> newValue, int newVersion,
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