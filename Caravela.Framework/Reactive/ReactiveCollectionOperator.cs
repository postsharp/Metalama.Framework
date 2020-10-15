using System.Collections;
using System.Collections.Generic;

namespace Caravela.Reactive
{
    internal abstract class ReactiveCollectionOperator<TSource, TResult> :
        ReactiveOperator<IEnumerable<TSource>, IReactiveCollectionObserver<TSource>, IEnumerable<TResult>,
            IReactiveCollectionObserver<TResult>>,
        IReactiveCollection<TResult>, IReactiveCollectionObserver<TSource>, IEnumerable<TResult>
    {
        public ReactiveCollectionOperator(IReactiveCollection<TSource> source) : base(source)
        {
        }

        IEnumerator<TResult> IEnumerable<TResult>.GetEnumerator()
        {
            return GetValue(CollectorToken).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetValue(CollectorToken).GetEnumerator();
        }


        void IReactiveCollectionObserver<TSource>.OnItemAdded(IReactiveSubscription subscription, TSource item,
            int newVersion)
        {
            using var token = GetUpdateToken();
            OnSourceItemAdded(subscription, item, in token);
        }

        void IReactiveCollectionObserver<TSource>.OnItemRemoved(IReactiveSubscription subscription, TSource item,
            int newVersion)
        {
            using var token = GetUpdateToken();
            OnSourceItemRemoved(subscription, item, in token);
        }

        void IReactiveCollectionObserver<TSource>.OnItemReplaced(IReactiveSubscription subscription, TSource oldItem,
            TSource newItem, int newVersion)
        {
            using var token = GetUpdateToken();
            OnSourceItemReplaced(subscription, oldItem, newItem, in token);
        }

        protected internal override IReactiveSubscription SubscribeToSource()
        {
            return Source.AddObserver(this);
        }


        protected abstract void OnSourceItemAdded(IReactiveSubscription sourceSubscription, TSource item,
            in UpdateToken updateToken);

        protected abstract void OnSourceItemRemoved(IReactiveSubscription sourceSubscription, TSource item,
            in UpdateToken updateToken);

        protected abstract void OnSourceItemReplaced(IReactiveSubscription sourceSubscription, TSource oldItem,
            TSource newItem, in UpdateToken updateToken);
    }
}