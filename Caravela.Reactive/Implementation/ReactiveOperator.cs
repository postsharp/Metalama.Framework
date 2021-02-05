#region


#endregion

using System;
using System.Diagnostics;
using System.Threading;

namespace Caravela.Reactive.Implementation
{

    public abstract class ReactiveOperator<TSource, TSourceObserver, TResult, TResultObserver> : 
        BaseReactiveOperator<TSource, TSourceObserver, TResult, TResultObserver>,
       IReactiveSource<TResult, TResultObserver>,
       IReactiveObserver<TSource>,
       IReactiveCollector
       where TSourceObserver : class, IReactiveObserver<TSource>
       where TResultObserver : class, IReactiveObserver<TResult>

    {
        private SpinLock _lock;

        protected ReactiveOperator( IReactiveSource<TSource, TSourceObserver> source ) : base( source )
        {
        }


        IReactiveVersionedValue<TResult> IReactiveSource<TResult>.GetVersionedValue(
            in ReactiveCollectorToken observerToken )
        {
            return this.GetVersionedValue( observerToken );
        }

        protected new IReactiveSource<TSource,TSourceObserver> Source => (IReactiveSource < TSource,TSourceObserver>) base.Source;


        /// <summary>
        /// Evaluates the function (i.e. the operator) and returns its value.
        /// </summary>
        /// <param name="source">Source value.</param>
        /// <returns>Result value.</returns>
        protected abstract ReactiveOperatorResult<TResult> EvaluateFunction( TSource source );

        public override bool IsImmutable
        {
            get
            {
                if ( !this.Source.IsImmutable )
                {
                    return false;
                }

                if ( this._result.Value.Version == 0 )
                {
                    // We're already evaluating the function so we can't do it again.
                    // To be safe, say we're mutable for now. 
                    if ( this._lock.IsHeldByCurrentThread )
                    {
                        return false;
                    }

                    // The function has never been evaluated, so dependencies were not collected.
                    this.EnsureFunctionEvaluated();
                }


                return this._dependencies.IsEmpty;
            }
        }

        IReactiveObservable<TResultObserver> IReactiveSource<TResult, TResultObserver>.Observable => this;



        /// <summary>
        /// Gets an <c>IncrementalUpdateToken</c>, which allows to represent incremental changes.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        protected IncrementalUpdateToken GetIncrementalUpdateToken( int sourceVersion = -1 )
        {
            if ( this._currentUpdateStatus != IncrementalUpdateStatus.Default && this._currentUpdateStatus != IncrementalUpdateStatus.Disposed )
            {
                throw new InvalidOperationException();
            }

            var lockTaken = false;
            this._lock.Enter( ref lockTaken );

            Debug.Assert( lockTaken );

            return new IncrementalUpdateToken( this, sourceVersion );
        }

        private protected override void ReleaseLock()
        {
            this._lock.Exit();
        }




        public ReactiveVersionedValue<TResult> GetVersionedValue( in ReactiveCollectorToken observerToken = default )
        {
            this.EnsureFunctionEvaluated();

            // Take local copy of the result to guarantee consistency in the version number.
            var currentValue = this._result.Value;
            this.CollectDependencies( observerToken, currentValue.Version );

            return currentValue;
        }

        public TResult GetValue( in ReactiveCollectorToken observerToken ) => this.GetVersionedValue( observerToken ).Value;

        /// <summary>
        /// Evaluates the operator if necessary.
        /// </summary>
        protected void EnsureFunctionEvaluated()
        {
            if ( this._isFunctionResultDirty )
            // This lock will avoid concurrent evaluations and evaluations concurrent to updates.
            {
                var lockHeld = false;
                try
                {
                    this._lock.Enter( ref lockHeld );

                    if ( this._isFunctionResultDirty )
                    {
                        // Evaluate the source.
                        var sourceValue = this.Source.GetVersionedValue( this.ObserverToken );
                        var sideValues = sourceValue.SideValues;

                        if ( sourceValue.Version != this._sourceVersion || !this._dependencies.IsDirty() )
                        {
                            this._sourceVersion = sourceValue.Version;

                            this._dependencies.Clear();

                            var currentResult = this._result.Value;

                            // If the source has changed, we need to evaluate our function again.
                            var newResult = this.EvaluateFunction( sourceValue.Value );

                            if ( currentResult.Version == 0 || !this.AreEqual( currentResult.Value, newResult.Value ) )
                            {
                                // Our function gave a different result, so we increase our version number.

                                sideValues = sideValues.Combine( newResult.SideValues );


                                this._result.Value =
                                    new ReactiveVersionedValue<TResult>( newResult.Value, currentResult.Version + 1, sideValues );
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
                    if ( lockHeld )
                    {
                        this._lock.Exit();
                    }
                }
            }
        }

    }
}