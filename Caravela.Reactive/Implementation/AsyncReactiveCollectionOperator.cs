using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Caravela.Reactive.Implementation
{
    /// <summary>
    /// A base implementation of <see cref="ReactiveOperator{TSource,TSourceObserver,TResult,TResultObserver}"/> for collection operators.
    /// </summary>
    /// <typeparam name="TSource">Type of source items.</typeparam>
    /// <typeparam name="TResult">Type of result items.</typeparam>
    public abstract class AsyncReactiveCollectionOperator<TSource, TResult> :
        AsyncReactiveOperator<IEnumerable<TSource>, IReactiveCollectionObserver<TSource>, IEnumerable<TResult>, IReactiveCollectionObserver<TResult>>,
        IAsyncReactiveCollection<TResult>, IReactiveCollectionObserver<TSource>
    {
        protected AsyncReactiveCollectionOperator( IAsyncReactiveCollection<TSource> source, bool hasReactiveDependencies ) : base( source, hasReactiveDependencies )
        {
        }

        async void IReactiveCollectionObserver<TSource>.OnItemAdded( IReactiveSubscription subscription, TSource item, int newVersion )
        {
            if ( !this.ShouldProcessIncrementalChange )
            {
                return;
            }

            using var token = await this.GetIncrementalUpdateTokenAsync( newVersion );

            await this.OnSourceItemAddedAsync( subscription, item, token, default );
        }

        async void IReactiveCollectionObserver<TSource>.OnItemRemoved( IReactiveSubscription subscription, TSource item, int newVersion )
        {
            if ( !this.ShouldProcessIncrementalChange )
            {
                return;
            }

            using var token = await this.GetIncrementalUpdateTokenAsync( newVersion );

            await this.OnSourceItemRemovedAsync( subscription, item, token, default );
        }

        async void IReactiveCollectionObserver<TSource>.OnItemReplaced( IReactiveSubscription subscription, TSource oldItem, TSource newItem, int newVersion )
        {
            if ( !this.ShouldProcessIncrementalChange )
            {
                return;
            }

            using var token = await this.GetIncrementalUpdateTokenAsync( newVersion );

            await this.OnSourceItemReplacedAsync( subscription, oldItem, newItem, token, default );
        }

        protected override IReactiveSubscription? SubscribeToSource()
        {
            return this.Source.Observable.AddObserver( this );
        }

        protected abstract ValueTask OnSourceItemAddedAsync( IReactiveSubscription sourceSubscription, TSource item, IncrementalUpdateToken updateToken, CancellationToken cancellationToken );

        protected abstract ValueTask OnSourceItemRemovedAsync( IReactiveSubscription sourceSubscription, TSource item, IncrementalUpdateToken updateToken, CancellationToken cancellationToken );

        protected abstract ValueTask OnSourceItemReplacedAsync( IReactiveSubscription sourceSubscription, TSource oldItem, TSource newItem, IncrementalUpdateToken updateToken, CancellationToken cancellationToken );
    }
}