using System;
using System.Collections.Generic;
using System.Linq;

namespace Caravela.Reactive
{
    internal abstract class SelectManyOperator<TSource, TResult> : ReactiveCollectionOperator<TSource, TResult>,
        IReactiveCollectionObserver<TResult>
    {
        private IEnumerable<TResult> _results = null!;

        protected SelectManyOperator(IReactiveCollection<TSource> source) : base(source)
        {
        }


        void IReactiveCollectionObserver<TResult>.OnItemAdded(IReactiveSubscription subscription, TResult item,
            int newVersion)
        {
            using var updateToken = GetUpdateToken();

            AddItem(item, updateToken);
        }

        void IReactiveCollectionObserver<TResult>.OnItemRemoved(IReactiveSubscription subscription, TResult item,
            int newVersion)
        {
            using var updateToken = GetUpdateToken();

            RemoveItem(item, updateToken);
        }

        void IReactiveCollectionObserver<TResult>.OnItemReplaced(IReactiveSubscription subscription, TResult oldItem,
            TResult newItem, int newVersion)
        {
            using var updateToken = GetUpdateToken();

            RemoveItem(oldItem, updateToken);
            AddItem(newItem, updateToken);
        }

        void IReactiveObserver.OnValueInvalidated(IReactiveSubscription subscription,
            bool isBreakingChange)
        {
            throw new NotImplementedException();
        }

        void IReactiveObserver<IEnumerable<TResult>>.OnValueChanged(IReactiveSubscription subscription,
            IEnumerable<TResult> oldValue, IEnumerable<TResult> newValue, int newVersion,
            bool isBreakingChange)
        {
            throw new NotImplementedException();
        }


        protected abstract void UnfollowAll();
        protected abstract void Unfollow(TSource source);
        protected abstract void Follow(TSource source);

        protected abstract IEnumerable<TResult> GetItems(TSource arg);


        protected override bool EvaluateFunction(IEnumerable<TSource> source)
        {
            UnfollowAll();

            foreach (var s in source) Follow(s);

            _results = source.SelectMany(GetItems);

            return true;
        }


        private void AddItem(TResult addedItem, in UpdateToken updateToken)
        {
            updateToken.SignalChange();

            // We have a new item.

            foreach (var observer in Observers) observer.Observer.OnItemAdded(observer, addedItem, updateToken.Version);
        }

        private void RemoveItem(TResult removedItem, in UpdateToken updateToken)
        {
            updateToken.SignalChange();


            foreach (var observer in Observers)
                observer.Observer.OnItemRemoved(observer, removedItem, updateToken.Version);
        }

        private void AddSource(TSource source, in UpdateToken updateToken)
        {
            Follow(source);


            foreach (var newItem in GetItems(source)) AddItem(newItem, updateToken);
        }

        private void RemoveSource(TSource source, in UpdateToken updateToken)
        {
            Unfollow(source);

            foreach (var removedItem in GetItems(source)) RemoveItem(removedItem, updateToken);
        }


        protected override void OnSourceItemAdded(IReactiveSubscription sourceSubscription, TSource item,
            in UpdateToken updateToken)
        {
            AddSource(item, updateToken);
        }

        protected override void OnSourceItemRemoved(IReactiveSubscription sourceSubscription, TSource item,
            in UpdateToken updateToken)
        {
            RemoveSource(item, updateToken);
        }

        protected override void OnSourceItemReplaced(IReactiveSubscription sourceSubscription, TSource oldItem,
            TSource newItem, in UpdateToken updateToken)
        {
            RemoveSource(oldItem, updateToken);
            AddSource(newItem, updateToken);
        }


        protected override IEnumerable<TResult> GetFunctionResult()
        {
            return _results;
        }
    }
}