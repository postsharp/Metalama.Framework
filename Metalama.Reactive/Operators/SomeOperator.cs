// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using System.Linq;
using Metalama.Reactive.Implementation;

namespace Metalama.Reactive.Operators
{
    internal class SomeOperator<T> : ReactiveOperator<IEnumerable<T>, IReactiveCollectionObserver<T>, T, IReactiveObserver<T>>, IReactiveCollectionObserver<T>
    {
        private static readonly IEqualityComparer<T?> _equalityComparer = EqualityComparerFactory.GetEqualityComparer<T?>();
        private readonly Func<T, ReactiveCollectorToken, bool> _predicate;
        private readonly bool _orDefault;

        public SomeOperator( IReactiveSource<IEnumerable<T>, IReactiveCollectionObserver<T>> source, Func<T, bool> predicate, bool orDefault ) : base( source )
        {
            this._predicate = ReactiveCollectorToken.WrapWithDefaultToken( predicate );
            this._orDefault = orDefault;
        }

        protected override IReactiveSubscription? SubscribeToSource()
        {
            return this.Source.Observable.AddObserver( this );
        }

        protected override ReactiveOperatorResult<T> EvaluateFunction( IEnumerable<T> source )
        {
            return this._orDefault
                ? source.FirstOrDefault( arg => this._predicate( arg, this.ObserverToken ) )
                : source.First( arg => this._predicate( arg, this.ObserverToken ) );
        }

        void IReactiveCollectionObserver<T>.OnItemAdded( IReactiveSubscription subscription, T item, int newVersion )
        {
            if ( !this.ShouldProcessIncrementalChange )
            {
                return;
            }

            var oldResult = this.CachedValue;

            if ( _equalityComparer.Equals( oldResult, default ) && this._predicate( item, this.ObserverToken ) )
            {
                using var token = this.GetIncrementalUpdateToken( newVersion );

                token.SetValue( item );

                foreach ( var observer in this.Observers )
                {
                    observer.Observer.OnValueChanged( observer.Subscription, oldResult, item, token.NextVersion );
                }
            }
        }

        void IReactiveCollectionObserver<T>.OnItemRemoved( IReactiveSubscription subscription, T item, int newVersion )
        {
            if ( !this.ShouldProcessIncrementalChange )
            {
                return;
            }

            var oldResult = this.CachedValue;

            if ( _equalityComparer.Equals( oldResult, item ) && this._predicate( item, this.ObserverToken ) )
            {
                this.OnObserverBreakingChange();
            }
        }

        void IReactiveCollectionObserver<T>.OnItemReplaced( IReactiveSubscription subscription, T oldItem, T newItem, int newVersion )
        {

            if ( !this.ShouldProcessIncrementalChange )
            {
                return;
            }

            var oldResult = this.CachedValue;

            if ( _equalityComparer.Equals( oldResult, oldItem )
                && this._predicate( oldItem, this.ObserverToken ) )
            {
                this.OnObserverBreakingChange();
            }
        }
    }
}