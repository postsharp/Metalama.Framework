// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using System.Linq;
using Metalama.Reactive.Implementation;

namespace Metalama.Reactive.Operators
{
    internal abstract class SelectManyOperator<TSource, TCollection, TResult> : ReactiveCollectionOperator<TSource, TResult>,
        IReactiveCollectionObserver<TCollection>
    {
        protected Func<TSource, TCollection, ReactiveCollectorToken, TResult> ResultSelector { get; }

        protected SelectManyOperator( IReactiveCollection<TSource> source, Func<TSource, TCollection, TResult> resultSelector ) : base( source )
        {
            this.ResultSelector = ReactiveCollectorToken.WrapWithDefaultToken( resultSelector );
        }

        protected abstract TResult SelectResult( IReactiveSubscription subscription, TCollection item );

        void IReactiveCollectionObserver<TCollection>.OnItemAdded( IReactiveSubscription subscription, TCollection item, int newVersion )
        {
            if ( !this.ShouldProcessIncrementalChange )
            {
                return;
            }

            using var updateToken = this.GetIncrementalUpdateToken();

            this.AddItem( this.SelectResult( subscription, item ), updateToken );
        }

        void IReactiveCollectionObserver<TCollection>.OnItemRemoved( IReactiveSubscription subscription, TCollection item, int newVersion )
        {
            if ( !this.ShouldProcessIncrementalChange )
            {
                return;
            }

            using var updateToken = this.GetIncrementalUpdateToken( newVersion );

            this.RemoveItem( this.SelectResult( subscription, item ), updateToken );
        }

        void IReactiveCollectionObserver<TCollection>.OnItemReplaced( IReactiveSubscription subscription, TCollection oldItem, TCollection newItem, int newVersion )
        {
            if ( !this.ShouldProcessIncrementalChange )
            {
                return;
            }

            using var updateToken = this.GetIncrementalUpdateToken( newVersion );

            this.RemoveItem( this.SelectResult( subscription, oldItem ), updateToken );
            this.AddItem( this.SelectResult( subscription, newItem ), updateToken );
        }

        void IReactiveObserver.OnValueInvalidated(
            IReactiveSubscription subscription,
            bool isBreakingChange )
        {
            if ( isBreakingChange )
            {
                this.OnObserverBreakingChange();
            }
        }

        void IReactiveObserver<IEnumerable<TCollection>>.OnValueChanged(
            IReactiveSubscription subscription,
            IEnumerable<TCollection> oldValue,
            IEnumerable<TCollection> newValue,
            int newVersion,
            bool isBreakingChange )
        {
            if ( isBreakingChange )
            {
                this.OnObserverBreakingChange();
            }
        }

        protected abstract void UnfollowAll();

        protected abstract void Unfollow( TSource source );

        protected abstract void Follow( TSource source );

        protected abstract IEnumerable<TResult> GetItems( TSource arg );

        protected override ReactiveOperatorResult<IEnumerable<TResult>> EvaluateFunction( IEnumerable<TSource> source )
        {
            this.UnfollowAll();

            foreach ( var s in source )
            {
                this.Follow( s );
            }

            return new( source.SelectMany( this.GetItems ) );
        }

        private void AddItem( TResult addedItem, in IncrementalUpdateToken updateToken )
        {
            updateToken.SetBreakingChange();

            // We have a new item.

            foreach ( var observer in this.Observers )
            {
                observer.Observer.OnItemAdded( observer.Subscription, addedItem, updateToken.NextVersion );
            }
        }

        private void RemoveItem( TResult removedItem, in IncrementalUpdateToken updateToken )
        {
            updateToken.SetBreakingChange();

            foreach ( var observer in this.Observers )
            {
                observer.Observer.OnItemRemoved( observer.Subscription, removedItem, updateToken.NextVersion );
            }
        }

        private void AddSource( TSource source, in IncrementalUpdateToken updateToken )
        {
            this.Follow( source );

            foreach ( var newItem in this.GetItems( source ) )
            {
                this.AddItem( newItem, updateToken );
            }
        }

        private void RemoveSource( TSource source, in IncrementalUpdateToken updateToken )
        {
            this.Unfollow( source );

            foreach ( var removedItem in this.GetItems( source ) )
            {
                this.RemoveItem( removedItem, updateToken );
            }
        }

        protected override void OnSourceItemAdded( IReactiveSubscription sourceSubscription, TSource item, in IncrementalUpdateToken updateToken )
        {
            this.AddSource( item, updateToken );
        }

        protected override void OnSourceItemRemoved( IReactiveSubscription sourceSubscription, TSource item, in IncrementalUpdateToken updateToken )
        {
            this.RemoveSource( item, updateToken );
        }

        protected override void OnSourceItemReplaced( IReactiveSubscription sourceSubscription, TSource oldItem, TSource newItem, in IncrementalUpdateToken updateToken )
        {
            this.RemoveSource( oldItem, updateToken );
            this.AddSource( newItem, updateToken );
        }
    }
}