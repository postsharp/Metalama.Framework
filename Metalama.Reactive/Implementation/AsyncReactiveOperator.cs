// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Metalama.Reactive.Implementation
{

    public abstract class AsyncReactiveOperator<TSource, TSourceObserver, TResult, TResultObserver> : BaseReactiveOperator<TSource, TSourceObserver, TResult, TResultObserver>,
        IAsyncReactiveSource<TResult, TResultObserver>,
        IReactiveObserver<TSource>,
        IReactiveCollector
        where TSourceObserver : class, IReactiveObserver<TSource>
        where TResultObserver : class, IReactiveObserver<TResult>
    {
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim( 1 );

        protected AsyncReactiveOperator( IAsyncReactiveSource<TSource, TSourceObserver> source, bool hasReactiveDependencies ) : base( source )
        {
            // We cannot determine synchronously if we have dependencies, so the caller has to specify.
            if ( !hasReactiveDependencies )
            {
                this._dependencies.Disable();
            }
        }

        protected new IAsyncReactiveSource<TSource, TSourceObserver> Source => (IAsyncReactiveSource<TSource, TSourceObserver>) base.Source;

        public override bool IsImmutable
        {
            get
            {
                if ( !this.Source.IsImmutable )
                {
                    return false;
                }

                return this._dependencies.IsEmpty;
            }
        }

        IReactiveObservable<TResultObserver> IAsyncReactiveSource<TResult, TResultObserver>.Observable => this;

        bool IReactiveSource.IsMaterialized => throw new NotImplementedException();

        bool IReactiveSource.IsImmutable => throw new NotImplementedException();

        /// <summary>
        /// Evaluates the function (i.e. the operator) and returns its value.
        /// </summary>
        /// <param name="source">Source value.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Result value.</returns>
        protected abstract ValueTask<ReactiveOperatorResult<TResult>> EvaluateFunctionAsync( TSource source, CancellationToken cancellationToken );

        public async ValueTask<TResult> GetValueAsync( ReactiveCollectorToken observerToken, CancellationToken cancellationToken )
            => (await this.GetVersionedValueAsync( observerToken, cancellationToken )).Value;

        public async ValueTask<IReactiveVersionedValue<TResult>> GetVersionedValueAsync( ReactiveCollectorToken observerToken, CancellationToken cancellationToken )
        {
            await this.EnsureFunctionEvaluatedAsync( cancellationToken );

            // Take local copy of the result to guarantee consistency in the version number.
            var currentValue = this._result.Value;
            this.CollectDependencies( observerToken, currentValue.Version );

            return currentValue;
        }

        /// <summary>
        /// Evaluates the operator if necessary.
        /// </summary>
        protected async ValueTask EnsureFunctionEvaluatedAsync( CancellationToken cancellationToken )
        {
            if ( this._isFunctionResultDirty )
            {
                // This lock will avoid concurrent evaluations and evaluations concurrent to updates.

                try
                {
                    await this._semaphore.WaitAsync( cancellationToken );

                    if ( this._isFunctionResultDirty )
                    {
                        // Evaluate the source.
                        var sourceValue = await this.Source.GetVersionedValueAsync( this.ObserverToken, cancellationToken );
                        var sideValues = sourceValue.SideValues;

                        if ( sourceValue.Version != this._sourceVersion || !this._dependencies.IsDirty() )
                        {
                            this._sourceVersion = sourceValue.Version;

                            this._dependencies.Clear();

                            var currentValue = this._result.Value;

                            // If the source has changed, we need to evaluate our function again.
                            var newResult = await this.EvaluateFunctionAsync( sourceValue.Value, cancellationToken );

                            if ( currentValue.Version == 0 || !this.AreEqual( currentValue.Value, newResult.Value ) )
                            {
                                // Our function gave a different result, so we increase our version number.

                                sideValues = sideValues.Combine( newResult.SideValues );

                                this._result.Value =
                                    new ReactiveVersionedValue<TResult>( newResult.Value, currentValue.Version + 1, sideValues );
                            }

                            // If the function has not produced dependencies on first execution, we forbid to produce them later.
                            // This makes sure we can implement the IsImmutable property reliably.
                            if ( this._dependencies.IsEmpty && this.Source.IsImmutable )
                            {
                                this._dependencies.Disable();
                            }
                        }

                        this._isFunctionResultDirty = false;
                    }

                    this.EnsureSubscribedToSource();
                }
                finally
                {
                    this._semaphore.Release();
                }
            }
        }

        /// <summary>
        /// Gets an <c>IncrementalUpdateToken</c>, which allows to represent incremental changes.
        /// </summary>
        /// <returns></returns>
        protected async ValueTask<IncrementalUpdateToken> GetIncrementalUpdateTokenAsync( int sourceVersion = -1, CancellationToken cancellationToken = default )
        {
            if ( this._currentUpdateStatus != IncrementalUpdateStatus.Default && this._currentUpdateStatus != IncrementalUpdateStatus.Disposed )
            {
                throw new InvalidOperationException();
            }

            await this._semaphore.WaitAsync( cancellationToken );

            return new IncrementalUpdateToken( this, sourceVersion );
        }

        private protected override void ReleaseLock()
        {
            this._semaphore.Release();
        }

        ValueTask<TResult> IAsyncReactiveSource<TResult>.GetValueAsync( ReactiveCollectorToken observerToken, CancellationToken cancellationToken )
        {
            throw new NotImplementedException();
        }

        ValueTask<IReactiveVersionedValue<TResult>> IAsyncReactiveSource<TResult>.GetVersionedValueAsync( ReactiveCollectorToken observerToken, CancellationToken cancellationToken )
        {
            throw new NotImplementedException();
        }
    }
}