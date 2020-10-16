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
        private IEnumerable<TResult> _results;

        protected SelectManyOperator(IReactiveCollection<TSource> source) : base(source)
        {
        }


        void IReactiveCollectionObserver<TResult>.OnItemAdded(IReactiveSubscription subscription, TResult item,
            int newVersion)
        {
            if (!this.CanProcessIncrementalChange)
                return;
            
            using var updateToken = this.GetIncrementalUpdateToken();

            this.AddItem(item, updateToken);
        }

        void IReactiveCollectionObserver<TResult>.OnItemRemoved(IReactiveSubscription subscription, TResult item,
            int newVersion)
        {
            if (!this.CanProcessIncrementalChange)
                return;
            
            using var updateToken = this.GetIncrementalUpdateToken();

            this.RemoveItem(item, updateToken);
        }

        void IReactiveCollectionObserver<TResult>.OnItemReplaced(IReactiveSubscription subscription, TResult oldItem,
            TResult newItem, int newVersion)
        {
            if (!this.CanProcessIncrementalChange)
                return;
            
            using var updateToken = this.GetIncrementalUpdateToken();

            this.RemoveItem(oldItem, updateToken);
            this.AddItem(newItem, updateToken);
        }

        void IReactiveObserver.OnValueInvalidated(IReactiveSubscription subscription,
            bool isBreakingChange)
        {
            if (isBreakingChange)
            {
                this.OnBreakingChange();
            }
        }

        void IReactiveObserver<IEnumerable<TResult>>.OnValueChanged(IReactiveSubscription subscription,
            IEnumerable<TResult> oldValue, IEnumerable<TResult> newValue, int newVersion,
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