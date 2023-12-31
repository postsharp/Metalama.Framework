// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Generic;
using System.Collections.Immutable;
using Metalama.Reactive.Implementation;

namespace Metalama.Reactive.Operators
{
    internal class ToListOperator<T> : ReactiveCollectionOperator<T, T>
    {
        private ImmutableList<T> _list = null!;

        public ToListOperator( IReactiveCollection<T> source ) : base( source )
        {
        }

        public override bool IsMaterialized => true;

        protected override ReactiveOperatorResult<IEnumerable<T>> EvaluateFunction( IEnumerable<T> source )
        {
            this._list = ImmutableList.CreateRange( source );
            return this._list;
        }

        protected override void OnSourceItemAdded( IReactiveSubscription sourceSubscription, T item, in IncrementalUpdateToken updateToken )
        {
            var oldList = this._list;

            this._list = this._list.Add( item );

            foreach ( var subscription in this.Observers )
            {
                subscription.Observer.OnItemAdded( subscription.Subscription, item, updateToken.NextVersion );
            }

            foreach ( var subscription in this.Observers.OfEnumerableType<T>() )
            {
                subscription.Observer.OnValueChanged( subscription.Subscription, oldList, this._list, updateToken.NextVersion );
            }

            updateToken.SetValue( this._list );
        }

        protected override void OnSourceItemRemoved( IReactiveSubscription sourceSubscription, T item, in IncrementalUpdateToken updateToken )
        {
            var oldList = this._list;

            this._list = this._list.Remove( item );

            foreach ( var subscription in this.Observers )
            {
                subscription.Observer.OnItemRemoved( subscription.Subscription, item, updateToken.NextVersion );
            }

            foreach ( var subscription in this.Observers.OfEnumerableType<T>() )
            {
                subscription.Observer.OnValueChanged( subscription.Subscription, oldList, this._list, updateToken.NextVersion );
            }

            updateToken.SetValue( this._list );
        }

        protected override void OnSourceItemReplaced( IReactiveSubscription sourceSubscription, T oldItem, T newItem, in IncrementalUpdateToken updateToken )
        {
            var oldList = this._list;

            this._list = this._list.Replace( oldItem, newItem );

            foreach ( var subscription in this.Observers )
            {
                subscription.Observer.OnItemReplaced( subscription.Subscription, oldItem, newItem, updateToken.NextVersion );
            }

            foreach ( var subscription in this.Observers.OfEnumerableType<T>() )
            {
                subscription.Observer.OnValueChanged( subscription.Subscription, oldList, this._list, updateToken.NextVersion );
            }

            updateToken.SetValue( this._list );
        }
    }
}