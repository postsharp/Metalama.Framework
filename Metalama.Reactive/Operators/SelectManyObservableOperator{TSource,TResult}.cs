// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;

namespace Caravela.Reactive.Operators
{

    internal sealed class SelectManyObservableOperator<TSource, TResult> : SelectManyObservableOperatorBase<TSource, TResult, TResult>
    {
        public SelectManyObservableOperator(
            IReactiveCollection<TSource> source, Func<TSource, IReactiveCollection<TResult>> collectionSelector )
            : base( source, collectionSelector, ( _, result ) => result )
        {
        }

        protected override IEnumerable<TResult> GetItems( TSource arg )
        {
            return this.CollectionSelector( arg, this.ObserverToken ).GetValue( this.ObserverToken );
        }
    }
}