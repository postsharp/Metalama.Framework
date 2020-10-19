#region

using System;
using System.Collections.Generic;
using System.Collections.Immutable;

#endregion

namespace Caravela.Reactive
{
    internal abstract class SelectManyImmutableOperatorBase<TSource, TCollection, TResult> : SelectManyOperator<TSource, TCollection, TResult>
    {
        protected Func<TSource, ReactiveObserverToken, IImmutableList<TCollection>> CollectionSelector { get; }

        public SelectManyImmutableOperatorBase(
            IReactiveCollection<TSource> source,
            Func<TSource, IImmutableList<TCollection>> collectionSelector,
            Func<TSource, TCollection, TResult> resultSelector)
            : base(source, resultSelector)
        {
            CollectionSelector = ReactiveObserverToken.WrapWithDefaultToken(collectionSelector);
        }

        protected override TResult SelectResult(IReactiveSubscription subscription, TCollection item) => 
            throw new NotSupportedException();

        protected override void UnfollowAll()
        {
        }

        protected override void Unfollow(TSource source)
        {
        }

        protected override void Follow(TSource source)
        {
        }
    }

    internal sealed class SelectManyImmutableOperator<TSource, TResult> : SelectManyImmutableOperatorBase<TSource, TResult, TResult>
    {
        public SelectManyImmutableOperator(
            IReactiveCollection<TSource> source, Func<TSource, IImmutableList<TResult>> collectionSelector)
            : base(source, collectionSelector, (source, result) => result)
        {
        }

        protected override IEnumerable<TResult> GetItems(TSource arg)
        {
            return CollectionSelector(arg, this.ObserverToken);
        }
    }

    internal sealed class SelectManyImmutableOperator<TSource, TCollection, TResult> : SelectManyImmutableOperatorBase<TSource, TCollection, TResult>
    {
        public SelectManyImmutableOperator(
            IReactiveCollection<TSource> source,
            Func<TSource, IImmutableList<TCollection>> collectionSelector,
            Func<TSource, TCollection, TResult> resultSelector)
            : base(source, collectionSelector, resultSelector)
        {
        }

        protected override IEnumerable<TResult> GetItems(TSource arg)
        {
            foreach (var item in CollectionSelector(arg, this.ObserverToken))
            {
                yield return ResultSelector(arg, item, this.ObserverToken);
            }
        }
    }
}