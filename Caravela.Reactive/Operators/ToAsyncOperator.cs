using Caravela.Reactive.Implementation;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Caravela.Reactive.Operators
{
    class ToAsyncOperator<TValue, TObserver> : IAsyncReactiveSource<TValue, TObserver>
         where TObserver : IReactiveObserver<TValue>
    {
        readonly IReactiveSource<TValue, TObserver> _source;

        public ToAsyncOperator( IReactiveSource<TValue, TObserver> source )
        {
            this._source = source;
        }

        int IReactiveObservable<TObserver>.Version => this._source.Version;

        object IReactiveObservable<TObserver>.Object => this._source.Object;

        bool IReactiveSource.IsMaterialized => this._source.IsMaterialized;

        bool IReactiveSource.IsImmutable => this._source.IsImmutable;

        IReactiveSubscription? IReactiveObservable<TObserver>.AddObserver( TObserver observer ) => this._source.AddObserver( observer );

        bool IReactiveObservable<TObserver>.RemoveObserver( IReactiveSubscription subscription ) => this._source.RemoveObserver( subscription );


        ValueTask<TValue> IAsyncReactiveSource<TValue>.GetValueAsync( ReactiveObserverToken observerToken, CancellationToken cancellationToken ) 
            => new ValueTask<TValue>( this._source.GetValue( observerToken ) );


        ValueTask<IReactiveVersionedValue<TValue>> IAsyncReactiveSource<TValue>.GetVersionedValueAsync( ReactiveObserverToken observerToken, CancellationToken cancellationToken ) 
            => new ValueTask<IReactiveVersionedValue<TValue>>( this._source.GetVersionedValue( observerToken ) );
        

    }
}
