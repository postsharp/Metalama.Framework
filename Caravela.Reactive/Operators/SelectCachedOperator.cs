using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Caravela.Reactive.Implementation;

namespace Caravela.Reactive.Operators
{
    internal class SelectCachedOperator<TSource, TResult> : ReactiveCollectionOperator<TSource, TResult>
        where TSource : class
        where TResult : class
    {
        private static readonly IEqualityComparer<TSource> _sourceEqualityComparer =
            EqualityComparerFactory.GetEqualityComparer<TSource>();

        private static readonly IEqualityComparer<TResult> _resultEqualityComparer = EqualityComparer<TResult>.Default;
        private readonly Func<TSource, ReactiveCollectorToken, TResult> _func;
        private readonly ConditionalWeakTable<TSource, TResult> _map = new ConditionalWeakTable<TSource, TResult>();

        public SelectCachedOperator( IReactiveCollection<TSource> source, Func<TSource, TResult> func )
            : base( source )
        {
            this._func = ReactiveCollectorToken.WrapWithDefaultToken( func );
        }

        public override bool IsMaterialized => false;

        private TResult Selector( TSource s ) => this._map.GetValue( s, k => this._func( k, this.ObserverToken ) );

        protected override ReactiveOperatorResult<IEnumerable<TResult>> EvaluateFunction( IEnumerable<TSource> source )
        {
            return new( source.Select( this.Selector ) );
        }

        protected override void OnSourceItemAdded( IReactiveSubscription sourceSubscription, TSource item, in IncrementalUpdateToken updateToken )
        {
            var outItem = this.Selector( item );

            updateToken.SetBreakingChange();

            foreach ( var subscription in this.Observers )
            {
                subscription.Observer.OnItemAdded( subscription.Subscription, outItem, updateToken.NextVersion );
            }
        }

        protected override void OnSourceItemRemoved( IReactiveSubscription sourceSubscription, TSource item, in IncrementalUpdateToken updateToken )
        {
            if ( !this._map.TryGetValue( item, out var outItem ) )
            {
                return;
            }

            updateToken.SetBreakingChange();

            foreach ( var subscription in this.Observers )
            {
                subscription.Observer.OnItemRemoved( subscription.Subscription, outItem, updateToken.NextVersion );
            }
        }

        protected override void OnSourceItemReplaced( IReactiveSubscription sourceSubscription, TSource oldItem, TSource newItem, in IncrementalUpdateToken updateToken )
        {
            if ( _sourceEqualityComparer.Equals( oldItem, newItem ) )
            {
                return;
            }

            if ( !this._map.TryGetValue( oldItem, out var oldItemResult ) )
            {
                // Should not happen.
                return;
            }

            var newItemResult = this.Selector( newItem );

            if ( _resultEqualityComparer.Equals( oldItemResult, newItemResult ) )
            {
                return;
            }

            updateToken.SetBreakingChange();

            foreach ( var subscription in this.Observers )
            {
                subscription.Observer.OnItemRemoved( subscription.Subscription, oldItemResult, updateToken.NextVersion );
                subscription.Observer.OnItemAdded( subscription.Subscription, newItemResult, updateToken.NextVersion );
            }
        }
    }
}