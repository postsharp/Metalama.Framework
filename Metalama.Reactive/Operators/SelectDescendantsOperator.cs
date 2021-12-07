// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Caravela.Reactive.Implementation;

namespace Caravela.Reactive.Operators
{
    internal class SelectDescendantsOperator<T> : ReactiveCollectionOperator<T, T>
        where T : class
    {
        private readonly Func<T, ReactiveCollectorToken, IReactiveCollection<T>> _getChildrenFunc;
        private ImmutableDictionary<T, int>? _dictionary;

        private ImmutableDictionary<IReactiveCollection<T>, (IReactiveSubscription? Subscription, int Count)>
            _subscriptions =
                ImmutableDictionary<IReactiveCollection<T>, (IReactiveSubscription? Subscription, int Count)>.Empty;

        public SelectDescendantsOperator(
            IReactiveCollection<T> source,
            Func<T, IReactiveCollection<T>> getChildrenFunc ) : base( source )
        {
            this._getChildrenFunc = ReactiveCollectorToken.WrapWithDefaultToken( getChildrenFunc );
        }

        public override bool IsMaterialized => true;

        protected override ReactiveOperatorResult<IEnumerable<T>> EvaluateFunction( IEnumerable<T> source )
        {
            var builder = ImmutableDictionary.CreateBuilder<T, int>();

            void Iterate( T item )
            {
                builder.TryGetValue( item, out var count );
                builder[item] = count + 1;

                var recursiveSource = this._getChildrenFunc( item, this.ObserverToken );

                if ( this.Follow( recursiveSource ) )
                {
                    foreach ( var recursiveItem in recursiveSource.GetValue( this.ObserverToken ) )
                    {
                        Iterate( recursiveItem );
                    }
                }
            }

            foreach ( var item in source )
            {
                Iterate( item );
            }

            this._dictionary = builder.ToImmutable();
            return new( this._dictionary.Keys );
        }

        private bool Follow( IReactiveCollection<T> source )
        {
            if ( !this._subscriptions.TryGetValue( source, out var existing ) )
            {
                this._subscriptions = this._subscriptions.Add( source, (source.Observable.AddObserver( this ), 1) );
                return true;
            }
            else
            {
                this._subscriptions = this._subscriptions.SetItem( source, (existing.Subscription, existing.Count + 1) );
                return false;
            }
        }

        private bool Unfollow( IReactiveCollection<T> source )
        {
            if ( !this._subscriptions.TryGetValue( source, out var existing ) )
            {
                return false;
            }

            if ( existing.Count == 1 )
            {
                this._subscriptions = this._subscriptions.Remove( source );
                return true;
            }
            else
            {
                this._subscriptions = this._subscriptions.SetItem( source, (existing.Subscription, existing.Count - 1) );
                return false;
            }
        }

        private void AddItem( T item, ref ImmutableDictionary<T, int> newResult, IncrementalUpdateToken updateToken )
        {
            // We need to duplicate the newResult parameter because local methods cannot access ref params of the parent method.
            void Iterate( T parentItem, ref ImmutableDictionary<T, int> result )
            {
                if ( !result.TryGetValue( parentItem, out var count ) )
                {
                    foreach ( var subscription in this.Observers )
                    {
                        subscription.Observer.OnItemAdded( subscription.Subscription, parentItem, updateToken.NextVersion );
                    }
                }

                result = result.SetItem( parentItem, count + 1 );

                var recursiveSource = this._getChildrenFunc( parentItem, this.ObserverToken );

                if ( this.Follow( recursiveSource ) )
                {
                    foreach ( var recursiveItem in recursiveSource.GetValue( this.ObserverToken ) )
                    {
                        Iterate( recursiveItem, ref result );
                    }
                }
            }

            Iterate( item, ref newResult );
        }

        private void RemoveItem( T item, ref ImmutableDictionary<T, int> newResult, IncrementalUpdateToken updateToken )
        {
            // We need to duplicate the newResult parameter because local methods cannot access ref params of the parent method.
            void Iterate( T parentItem, ref ImmutableDictionary<T, int> result )
            {
                if ( !result.TryGetValue( parentItem, out var count ) )
                {
                    return;
                }

                if ( count == 1 )
                {

                    foreach ( var subscription in this.Observers )
                    {
                        subscription.Observer.OnItemRemoved( subscription.Subscription, parentItem, updateToken.NextVersion );
                    }

                    result = result.Remove( parentItem );
                }
                else
                {
                    result = result.SetItem( parentItem, count - 1 );
                }

                var recursiveSource = this._getChildrenFunc( parentItem, this.ObserverToken );

                if ( this.Unfollow( recursiveSource ) )
                {
                    foreach ( var recursiveItem in recursiveSource.GetValue( this.ObserverToken ) )
                    {
                        Iterate( recursiveItem, ref result );
                    }
                }
            }

            Iterate( item, ref newResult );
        }

        protected override void OnSourceItemAdded( IReactiveSubscription sourceSubscription, T item, in IncrementalUpdateToken updateToken )
        {
            var newResult = this._dictionary!;

            this.AddItem( item, ref newResult, updateToken );

            this._dictionary = newResult;

            updateToken.SetValue( newResult.Keys );
        }

        protected override void OnSourceItemRemoved( IReactiveSubscription sourceSubscription, T item, in IncrementalUpdateToken updateToken )
        {
            var newResult = this._dictionary!;

            this.RemoveItem( item, ref newResult, updateToken );

            this._dictionary = newResult;

            updateToken.SetValue( newResult.Keys );
        }

        protected override void OnSourceItemReplaced( IReactiveSubscription sourceSubscription, T oldItem, T newItem, in IncrementalUpdateToken updateToken )
        {
            var newResult = this._dictionary!;

            this.RemoveItem( oldItem, ref newResult, updateToken );
            this.AddItem( newItem, ref newResult, updateToken );

            this._dictionary = newResult;

            updateToken.SetValue( newResult.Keys );
        }
    }
}