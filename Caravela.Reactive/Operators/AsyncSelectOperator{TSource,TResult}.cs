using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Caravela.Reactive.Implementation;

namespace Caravela.Reactive.Operators
{
    internal class AsyncSelectOperator<TSource, TResult> : AsyncReactiveCollectionOperator<TSource, TResult>
    {
        private static readonly IEqualityComparer<TSource> _sourceEqualityComparer =
            EqualityComparerFactory.GetEqualityComparer<TSource>();

        private static readonly IEqualityComparer<TResult> _resultEqualityComparer = EqualityComparer<TResult>.Default;
        private readonly Func<TSource, ReactiveCollectorToken, CancellationToken, ValueTask<TResult>> _func;

        public AsyncSelectOperator( IAsyncReactiveCollection<TSource> source, Func<TSource, CancellationToken, ValueTask<TResult>> func, bool hasReactiveDependencies )
            : base( source, hasReactiveDependencies )
        {
            this._func = ReactiveCollectorToken.WrapWithDefaultToken( func );
        }

        protected override async ValueTask<ReactiveOperatorResult<IEnumerable<TResult>>> EvaluateFunctionAsync( IEnumerable<TSource> source, CancellationToken cancellationToken )
        {
            var builder = ImmutableList.CreateBuilder<TResult>();

            foreach ( var item in source )
            {
                builder.Add( await this._func( item, this.ObserverToken, cancellationToken ) );
            }

            return new( builder.ToImmutable() );
        }

        protected override async ValueTask OnSourceItemAddedAsync( IReactiveSubscription sourceSubscription, TSource item, IncrementalUpdateToken updateToken, CancellationToken cancellationToken )
        {
            var outItem = await this._func( item, this.ObserverToken, cancellationToken );

            updateToken.SetBreakingChange();

            foreach ( var subscription in this.Observers )
            {
                subscription.Observer.OnItemAdded( subscription.Subscription, outItem, updateToken.NextVersion );
            }
        }

        protected override async ValueTask OnSourceItemRemovedAsync( IReactiveSubscription sourceSubscription, TSource item, IncrementalUpdateToken updateToken, CancellationToken cancellationToken )
        {
            var outItem = await this._func( item, this.ObserverToken, cancellationToken );

            updateToken.SetBreakingChange();

            foreach ( var subscription in this.Observers )
            {
                subscription.Observer.OnItemRemoved( subscription.Subscription, outItem, updateToken.NextVersion );
            }
        }

        protected override async ValueTask OnSourceItemReplacedAsync( IReactiveSubscription sourceSubscription, TSource oldItem, TSource newItem, IncrementalUpdateToken updateToken, CancellationToken cancellationToken )
        {
            if ( _sourceEqualityComparer.Equals( oldItem, newItem ) )
            {
                return;
            }

            var oldItemResult = await this._func( oldItem, this.ObserverToken, cancellationToken );
            var newItemResult = await this._func( newItem, this.ObserverToken, cancellationToken );

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