#region

using System;
using System.Collections.Generic;
using System.Collections.Immutable;

#endregion

namespace Caravela.Reactive
{
    internal class SelectManyImmutableOperator<TSource, TResult> : SelectManyOperator<TSource, TResult>
    {
        private readonly Func<TSource, ReactiveCollectorToken, IEnumerable<TResult>> _func;

        public SelectManyImmutableOperator(IReactiveCollection<TSource> source,
            Func<TSource, ReactiveCollectorToken, IImmutableList<TResult>> func) : base(source)
        {
            this._func = func;
        }


        protected override void UnfollowAll()
        {
        }

        protected override void Unfollow(TSource source)
        {
        }

        protected override void Follow(TSource source)
        {
        }

        protected override IEnumerable<TResult> GetItems(TSource arg)
        {
            return this._func(arg, this.CollectorToken);
        }
    }
}