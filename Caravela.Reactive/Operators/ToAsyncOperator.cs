using System.Threading;
using System.Threading.Tasks;

namespace Caravela.Reactive.Operators
{
    class ToAsyncOperator<TValue, TObserver> : IAsyncReactiveSource<TValue, TObserver>, IReactiveObservable<TObserver>
         where TObserver : IReactiveObserver<TValue>
    {
        readonly IReactiveSource<TValue, TObserver> _source;

        public ToAsyncOperator( IReactiveSource<TValue, TObserver> source )
        {
            this._source = source;
        }

        int IReactiveObservable<TObserver>.Version => this._source.Observable.Version;

        IReactiveSource IReactiveObservable<TObserver>.Source => this._source.Observable.Source;

        bool IReactiveSource.IsMaterialized => this._source.IsMaterialized;

        bool IReactiveSource.IsImmutable => this._source.IsImmutable;

        IReactiveObservable<TObserver> IAsyncReactiveSource<TValue, TObserver>.Observable => this;

        IReactiveSubscription? IReactiveObservable<TObserver>.AddObserver( TObserver observer ) => this._source.Observable.AddObserver( observer );

        bool IReactiveObservable<TObserver>.RemoveObserver( IReactiveSubscription subscription ) => this._source.Observable.RemoveObserver( subscription );


        ValueTask<TValue> IAsyncReactiveSource<TValue>.GetValueAsync( ReactiveCollectorToken observerToken, CancellationToken cancellationToken ) 
            => new ValueTask<TValue>( this._source.GetValue( observerToken ) );


        ValueTask<IReactiveVersionedValue<TValue>> IAsyncReactiveSource<TValue>.GetVersionedValueAsync( ReactiveCollectorToken observerToken, CancellationToken cancellationToken ) 
            => new ValueTask<IReactiveVersionedValue<TValue>>( this._source.GetVersionedValue( observerToken ) );
        

    }
}
