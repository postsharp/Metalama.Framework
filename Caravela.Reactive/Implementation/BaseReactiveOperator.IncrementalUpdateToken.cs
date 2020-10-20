using System;
using System.Diagnostics;

namespace Caravela.Reactive.Implementation
{
    partial class BaseReactiveOperator<TSource, TSourceObserver, TResult, TResultObserver>
    {
        private protected enum IncrementalUpdateStatus
        {
            /// <summary>
            /// No change.
            /// </summary>
            Default,
            
            /// <summary>
            /// Has some change.
            /// </summary>
            HasChange,
            
            /// <summary>
            /// Has some change and a new value has been assigned.
            /// </summary>
            HasNewValue,
            
            /// <summary>
            /// IncrementalUpdateToken.Disposed was called.
            /// </summary>
            Disposed
        }
        
        
        /// <summary>
        /// Token passed to methods processing incremental changes.
        /// </summary>
        protected  readonly struct IncrementalUpdateToken : IDisposable
        {
            private readonly BaseReactiveOperator<TSource, TSourceObserver, TResult, TResultObserver> _parent;

            public IncrementalUpdateToken( BaseReactiveOperator<TSource, TSourceObserver, TResult, TResultObserver> parent,
                int sourceVersion)
            {
                this._parent = parent;
                parent._currentUpdateNewSourceVersion = sourceVersion;
                parent._currentUpdateStatus = IncrementalUpdateStatus.Default;
                parent._currentUpdateSideValues = default;
            }

            public void SetBreakingChange()
            {
                // We have an incremental change that breaks the stored value, so _parent.EvaluateFunction() must
                // be called again. However, observers don't need to resynchronize if they are able to process
                // the increment.

                if ( this._parent == null )
                    throw new InvalidOperationException();


                if ( this._parent._currentUpdateStatus == IncrementalUpdateStatus.Default )
                {
                    this._parent._currentUpdateStatus = IncrementalUpdateStatus.HasChange;
                }

                this._parent._isFunctionResultDirty = true;

                // We don't nullify the old result now because the current result may eventually be still valid if all versions 
                // end up being identical.
            }



            public void SetValue( TResult newResult )
            {
                if ( this._parent == null )
                    throw new InvalidOperationException();


                this._parent._currentUpdateStatus = IncrementalUpdateStatus.HasNewValue;
                this._parent._currentUpdateResult = newResult;
            }

            public void SetValue( TResult newResult, IReactiveSideValue sideValue)
            {
                this.SetValue( newResult );
                this._parent._currentUpdateSideValues = this._parent._currentUpdateSideValues.Combine( sideValue );
            }

            public void SetValue( TResult newResult, ReactiveSideValues sideValues )
            {
                this.SetValue( newResult );
                this._parent._currentUpdateSideValues = this._parent._currentUpdateSideValues.Combine( sideValues );
            }

            public int NextVersion => this._parent._result?.Version + 1 ?? 0;


            public void Dispose()
            {
                if (this._parent == null || this._parent._currentUpdateStatus == IncrementalUpdateStatus.Disposed)
                {
                    return;
                }


                if (this._parent._currentUpdateStatus != IncrementalUpdateStatus.Default)
                {
                    if (this._parent._currentUpdateStatus == IncrementalUpdateStatus.HasNewValue)
                    {
                        if (this._parent._currentUpdateNewSourceVersion >= 0)
                        {
                            this._parent._sourceVersion = this._parent._currentUpdateNewSourceVersion;
                        }

                        this._parent._result =
                            new ReactiveVersionedValue<TResult>(this._parent._currentUpdateResult!, this.NextVersion, this._parent._currentUpdateSideValues);
                    }
                    else
                    {
                        // If the function result is not dirty, the caller should have called SetNewValue.
                        Debug.Assert(this._parent._isFunctionResultDirty);
                    }


                    foreach (var subscription in this._parent._observers.WeaklyTyped())
                    {
                        subscription.Observer.OnValueInvalidated(subscription.Subscription, false);
                    }
                }

                this._parent._currentUpdateStatus = IncrementalUpdateStatus.Disposed;
                this._parent.ReleaseLock();

            }
        }

        private protected abstract void ReleaseLock();
    }
}