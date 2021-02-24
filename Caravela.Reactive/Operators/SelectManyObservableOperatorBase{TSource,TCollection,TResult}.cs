// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using Caravela.Reactive.Implementation;

namespace Caravela.Reactive.Operators
{
    internal abstract class SelectManyObservableOperatorBase<TSource, TCollection, TResult> : SelectManyOperator<TSource, TCollection, TResult>
    {
        protected Func<TSource, ReactiveCollectorToken, IReactiveCollection<TCollection>> CollectionSelector { get; }

        private readonly Dictionary<TSource, (IReactiveSubscription? Subscription, int Count)> _subscriptions
            = new( EqualityComparerFactory.GetEqualityComparer<TSource>() );

        private readonly Dictionary<IReactiveSubscription, TSource> _subscriptionsReverse = new();

        public SelectManyObservableOperatorBase(
            IReactiveCollection<TSource> source,
            Func<TSource, IReactiveCollection<TCollection>> collectionSelector,
            Func<TSource, TCollection, TResult> resultSelector ) : base( source, resultSelector )
        {
            this.CollectionSelector = ReactiveCollectorToken.WrapWithDefaultToken( collectionSelector );
        }

        protected override TResult SelectResult( IReactiveSubscription subscription, TCollection item ) =>
            this.ResultSelector( this._subscriptionsReverse[subscription], item, this.ObserverToken );

        protected override void UnfollowAll()
        {
            foreach ( var (subscription, _) in this._subscriptions.Values )
            {
                subscription?.Dispose();
            }

            this._subscriptionsReverse.Clear();
        }

        protected override void Unfollow( TSource source )
        {
            if ( this._subscriptions.TryGetValue( source, out var tuple ) )
            {
                if ( tuple.Count == 1 )
                {
                    tuple.Subscription?.Dispose();
                    this._subscriptions.Remove( source );
                    if ( tuple.Subscription != null )
                    {
                        this._subscriptionsReverse.Remove( tuple.Subscription );
                    }
                }
                else
                {
                    this._subscriptions[source] = (tuple.Subscription, tuple.Count - 1);
                }
            }
        }

        protected override void Follow( TSource source )
        {
            if ( this._subscriptions.TryGetValue( source, out var tuple ) )
            {
                this._subscriptions[source] = (tuple.Subscription, tuple.Count + 1);
            }
            else
            {
                var subscription = this.CollectionSelector( source, this.ObserverToken ).Observable.AddObserver( this );
                if ( subscription != null )
                {
                    this._subscriptions.Add( source, (subscription, 1) );
                    this._subscriptionsReverse.Add( subscription, source );
                }
            }
        }
    }
}