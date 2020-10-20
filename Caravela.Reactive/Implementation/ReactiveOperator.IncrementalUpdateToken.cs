using System;
using System.Diagnostics;

namespace Caravela.Reactive.Implementation
{
    partial class ReactiveOperator<TSource, TSourceObserver, TResult, TResultObserver>
    {
        private enum IncrementalUpdateStatus
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
            private readonly ReactiveOperator<TSource, TSourceObserver, TResult, TResultObserver> _parent;

            public IncrementalUpdateToken(ReactiveOperator<TSource, TSourceObserver, TResult, TResultObserver> parent,
                int sourceVersion)
            {
                this._parent = parent;
                parent._currentUpdateNewSourceVersion = sourceVersion;
                parent._currentUpdateStatus = IncrementalUpdateStatus.Default;

                bool lockTaken = false;
                this._parent._lock.Enter(ref lockTaken);
                
                Debug.Assert(lockTaken);
            }


            /// <summary>
            /// Signals that the update causes a change in the result.
            /// </summary>
            /// <param name="mustEvaluateFromSource"><c>True</c> if <see cref="ReactiveOperator{TSource,TSourceObserver,TResult,TResultObserver}.EvaluateFunction"/>
            /// must be called again, or <c>false</c> if the caller will call <see cref="SetNewValue"/>.</param>
            /// <exception cref="InvalidOperationException"></exception>
            public void SignalChange(bool mustEvaluateFromSource = false)
            {
                if (this._parent == null)
                    throw new InvalidOperationException();


                if (this._parent._currentUpdateStatus == IncrementalUpdateStatus.Default)
                {
                    this._parent._currentUpdateStatus = IncrementalUpdateStatus.HasChange;
                }

                if (mustEvaluateFromSource)
                {
                    // We have an incremental change that breaks the stored value, so _parent.EvaluateFunction() must
                    // be called again. However, observers don't need to resynchronize if they are able to process
                    // the increment.

                    this._parent._isFunctionResultDirty = true;

                    // We don't nullify the old result now because the current result may eventually be still valid if all versions 
                    // end up being identical.
                }
            }

            public void SetNewValue(TResult newResult)
            {
                if (this._parent == null)
                    throw new InvalidOperationException();


                this._parent._currentUpdateStatus = IncrementalUpdateStatus.HasNewValue;
                this._parent._currentUpdateResult = newResult;
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
                            new ReactiveVersionedValue<TResult>(this._parent._currentUpdateResult!, this.NextVersion);
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
                this._parent._lock.Exit();

            }
        }
    }
}