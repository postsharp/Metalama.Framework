// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Generic;
using System.Linq;
using Caravela.Reactive.Implementation;

namespace Caravela.Reactive.Operators
{
    internal class UnionOperator<T> : ReactiveCollectionOperator<T, T>
    {
        private readonly IReactiveCollection<T> _second;
        private IReactiveSubscription? _secondSubscription;

        public UnionOperator( IReactiveCollection<T> source, IReactiveCollection<T> second )
            : base( source )
        {
            this._second = second;
        }

        protected override ReactiveOperatorResult<IEnumerable<T>> EvaluateFunction( IEnumerable<T> source )
        {
            return new( source.Union( this._second.GetValue( this.ObserverToken ) ) );
        }

        protected override IReactiveSubscription? SubscribeToSource()
        {
            this._secondSubscription = this._second.Observable.AddObserver( this );
            return base.SubscribeToSource();
        }

        protected override void UnsubscribeFromSource()
        {
            base.UnsubscribeFromSource();
            this._secondSubscription?.Dispose();
            this._secondSubscription = null;
        }

        protected override void OnSourceItemAdded( IReactiveSubscription sourceSubscription, T item, in IncrementalUpdateToken updateToken )
        {
            updateToken.SetBreakingChange();

            foreach ( var subscription in this.Observers )
            {
                subscription.Observer.OnItemAdded( subscription.Subscription, item, updateToken.NextVersion );
            }
        }

        protected override void OnSourceItemRemoved( IReactiveSubscription sourceSubscription, T item, in IncrementalUpdateToken updateToken )
        {
            updateToken.SetBreakingChange();

            foreach ( var subscription in this.Observers )
            {
                subscription.Observer.OnItemRemoved( subscription.Subscription, item, updateToken.NextVersion );
            }
        }

        protected override void OnSourceItemReplaced( IReactiveSubscription sourceSubscription, T oldItem, T newItem, in IncrementalUpdateToken updateToken )
        {
            updateToken.SetBreakingChange();

            foreach ( var subscription in this.Observers )
            {
                subscription.Observer.OnItemRemoved( subscription.Subscription, oldItem, updateToken.NextVersion );
                subscription.Observer.OnItemAdded( subscription.Subscription, newItem, updateToken.NextVersion );
            }
        }

        protected override bool ShouldTrackDependency( IReactiveObservable<IReactiveObserver> source )
            => base.ShouldTrackDependency( source ) && source.Source != this._second;
    }
}