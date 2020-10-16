#region

using System;
using System.Collections.Generic;
using System.Collections.Immutable;

#endregion

namespace Caravela.Reactive
{
    internal abstract class SelectManyImmutableOperatorBase<TSource, TCollection, TResult> : SelectManyOperator<TSource, TCollection, TResult>
    {
        protected Func<TSource, ReactiveCollectorToken, IImmutableList<TCollection>> CollectionSelector { get; }

        public SelectManyImmutableOperatorBase(
            IReactiveCollection<TSource> source,
            Func<TSource, ReactiveCollectorToken, IImmutableList<TCollection>> collectionSelector,
            Func<TSource, TCollection, ReactiveCollectorToken, TResult> resultSelector)
            : base(source, resultSelector)
        {
            CollectionSelector = collectionSelector;
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
            IReactiveCollection<TSource> source, Func<TSource, ReactiveCollectorToken, IImmutableList<TResult>> collectionSelector)
            : base(source, collectionSelector, (source, result, token) => result)
        {
        }

        protected override IEnumerable<TResult> GetItems(TSource arg)
        {
            return CollectionSelector(arg, CollectorToken);
        }
    }

    internal sealed class SelectManyImmutableOperator<TSource, TCollection, TResult> : SelectManyImmutableOperatorBase<TSource, TCollection, TResult>
    {
        public SelectManyImmutableOperator(
            IReactiveCollection<TSource> source,
            Func<TSource, ReactiveCollectorToken, IImmutableList<TCollection>> collectionSelector,
            Func<TSource, TCollection, ReactiveCollectorToken, TResult> resultSelector)
            : base(source, collectionSelector, resultSelector)
        {
        }

        protected override IEnumerable<TResult> GetItems(TSource arg)
        {
            foreach (var item in CollectionSelector(arg, CollectorToken))
            {
                yield return ResultSelector(arg, item, CollectorToken);
            }
        }
    }
}