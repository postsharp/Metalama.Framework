// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections;
using System.Collections.Generic;

namespace Metalama.Reactive.Implementation
{
    /// <summary>
    /// A base implementation of <see cref="ReactiveOperator{TSource,TSourceObserver,TResult,TResultObserver}"/> for collection operators.
    /// </summary>
    /// <typeparam name="TSource">Type of source items.</typeparam>
    /// <typeparam name="TResult">Type of result items.</typeparam>
    public abstract class ReactiveCollectionOperator<TSource, TResult> :
        ReactiveOperator<IEnumerable<TSource>, IReactiveCollectionObserver<TSource>, IEnumerable<TResult>,
            IReactiveCollectionObserver<TResult>>,
        IReactiveCollection<TResult>, IReactiveCollectionObserver<TSource>, IEnumerable<TResult>
    {
        protected ReactiveCollectionOperator( IReactiveCollection<TSource> source ) : base( source )
        {
        }

        IEnumerator<TResult> IEnumerable<TResult>.GetEnumerator()
        {
            return this.GetValue( this.ObserverToken ).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetValue( this.ObserverToken ).GetEnumerator();
        }

        void IReactiveCollectionObserver<TSource>.OnItemAdded( IReactiveSubscription subscription, TSource item, int newVersion )
        {
            if ( !this.ShouldProcessIncrementalChange )
            {
                return;
            }

            using var token = this.GetIncrementalUpdateToken( newVersion );

            this.OnSourceItemAdded( subscription, item, in token );
        }

        void IReactiveCollectionObserver<TSource>.OnItemRemoved( IReactiveSubscription subscription, TSource item, int newVersion )
        {
            if ( !this.ShouldProcessIncrementalChange )
            {
                return;
            }

            using var token = this.GetIncrementalUpdateToken( newVersion );

            this.OnSourceItemRemoved( subscription, item, in token );
        }

        void IReactiveCollectionObserver<TSource>.OnItemReplaced( IReactiveSubscription subscription, TSource oldItem, TSource newItem, int newVersion )
        {
            if ( !this.ShouldProcessIncrementalChange )
            {
                return;
            }

            using var token = this.GetIncrementalUpdateToken( newVersion );

            this.OnSourceItemReplaced( subscription, oldItem, newItem, in token );
        }

        protected override IReactiveSubscription? SubscribeToSource()
        {
            return this.Source.Observable.AddObserver( this );
        }

        protected abstract void OnSourceItemAdded( IReactiveSubscription sourceSubscription, TSource item, in IncrementalUpdateToken updateToken );

        protected abstract void OnSourceItemRemoved( IReactiveSubscription sourceSubscription, TSource item, in IncrementalUpdateToken updateToken );

        protected abstract void OnSourceItemReplaced( IReactiveSubscription sourceSubscription, TSource oldItem, TSource newItem, in IncrementalUpdateToken updateToken );
    }
}