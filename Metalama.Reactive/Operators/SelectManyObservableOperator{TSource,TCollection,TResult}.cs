// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;

namespace Metalama.Reactive.Operators
{

    internal sealed class SelectManyObservableOperator<TSource, TCollection, TResult> : SelectManyObservableOperatorBase<TSource, TCollection, TResult>
    {
        public SelectManyObservableOperator(
            IReactiveCollection<TSource> source,
            Func<TSource, IReactiveCollection<TCollection>> collectionSelector,
            Func<TSource, TCollection, TResult> resultSelector )
            : base( source, collectionSelector, resultSelector )
        {
        }

        protected override IEnumerable<TResult> GetItems( TSource arg )
        {
            foreach ( var item in this.CollectionSelector( arg, this.ObserverToken ).GetValue( this.ObserverToken ) )
            {
                yield return this.ResultSelector( arg, item, this.ObserverToken );
            }
        }
    }
}