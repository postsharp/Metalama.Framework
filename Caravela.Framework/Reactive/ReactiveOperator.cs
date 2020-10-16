#region

using System;
using System.Diagnostics;
using System.Threading;

#endregion

namespace Caravela.Reactive
{
    internal abstract class ReactiveOperator<TSource, TSourceObserver, TResult, TResultObserver> :
        IReactiveSource<TResult, TResultObserver>,
        IReactiveObserver<TSource>,
        IReactiveTokenCollector, IGroupByOperator
        where TSourceObserver : IReactiveObserver<TSource>
        where TResultObserver : class, IReactiveObserver<TResult>

    {
        private DependencyList _dependencies;
        private bool _currentUpdateHasChange;
        private volatile int _inputVersion = -1;
        private volatile bool _isFunctionResultDirty = true;
        private volatile int _version = -1;
        private DependencyObservable? _dependencyObservable;

        protected ReactiveOperator(IReactiveSource<TSource, TSourceObserver> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            this.Source = source;
            this.Observers = new ObserverList<TResultObserver>(this);
            this._dependencies = new DependencyList(this);
        }

        protected ObserverList<TResultObserver> Observers { get; }
        private object Sync => this.Observers;


        private IReactiveSubscription? SubscriptionToSource { get; set; }


        protected IReactiveSource<TSource, TSourceObserver> Source { get; }

        
        protected ReactiveCollectorToken CollectorToken => new ReactiveCollectorToken(this);


        void IReactiveObserver.OnValueInvalidated(IReactiveSubscription subscription, bool isBreakingChange)
        {
            if (subscription == this.SubscriptionToSource)
            {
                this.OnValueChanged(isBreakingChange);
            }
            else
            {
                this.OnOtherValueInvalidated(subscription, isBreakingChange);
            }
        }

        bool IReactiveObserver.HasPathToSource(object source)
        {
            return this.Observers.HasPathToObserver(source);
        }


        void IReactiveObserver<TSource>.OnValueChanged(IReactiveSubscription subscription, TSource oldValue,
            TSource newValue, int newVersion, bool isBreakingChange)
        {
            this.OnValueChanged(isBreakingChange);
        }

        public void Dispose()
        {
            this.UnsubscribeFromSource();
        }


        IReactiveSubscription IReactiveObservable<TResultObserver>.AddObserver(TResultObserver observer)
        {
            this.EnsureSubscribedToSource();

            if (this.IsImmutable)
            {
                return null;
            }

            return this.Observers.AddObserver(observer);
        }

        bool IReactiveObservable<TResultObserver>.RemoveObserver(IReactiveSubscription subscription)
        {
            if (subscription == null)
            {
                return false;
            }

            return this.Observers.RemoveObserver(subscription);
        }

        bool IReactiveDebugging.HasPathToObserver(object observer)
        {
            return observer == this || this.Observers.HasPathToObserver(observer);
        }

        public bool IsImmutable => this.Source.IsImmutable && this._dependencies.IsEmpty;

        public int Version => this._version;

        public TResult GetValue(in ReactiveCollectorToken collectorToken)
        {
            return this.GetValueImpl(collectorToken).Value;
        }

        IReactiveVersionedValue<TResult> IReactiveSource<TResult, TResultObserver>.
            GetVersionedValue(in ReactiveCollectorToken collectorToken)
        {
            return this.GetValueImpl(collectorToken);
        }

        public virtual bool IsMaterialized => false;

        object IReactiveObservable<TResultObserver>.Object => this;

        protected virtual bool ShouldTrackDependency(IReactiveObservable<IReactiveObserver> observable)
            => observable.Object != this && observable.Object != this.Source;

        void IReactiveTokenCollector.AddDependency(IReactiveObservable<IReactiveObserver> observable)
        {
            if (this.ShouldTrackDependency(observable))
            {
                this._dependencies.Add(observable);
            }
        }

        protected internal abstract IReactiveSubscription SubscribeToSource();

        protected internal void EnsureSubscribedToSource()
        {
            if (this.SubscriptionToSource == null)
            {
                this.SubscriptionToSource = this.SubscribeToSource();
            }
        }

        void IGroupByOperator.EnsureSubscribedToSource() => EnsureSubscribedToSource();

        protected internal virtual void UnsubscribeFromSource()
        {
            this.SubscriptionToSource?.Dispose();
            this.SubscriptionToSource = default;
        }

        protected abstract bool EvaluateFunction(TSource source);

        protected abstract TResult GetFunctionResult();


        protected bool CanProcessIncrementalChange => this._version >= 0;


        protected UpdateToken GetIncrementalUpdateToken()
        {
            return new UpdateToken(this);
        }


        private ReactiveVersionedValue<TResult> GetValueImpl(in ReactiveCollectorToken collectorToken)
        {
            this.EnsureFunctionEvaluated();
            
            // Collect after evaluation so that the version number is updated.
            collectorToken.Collector?.AddDependency((this._dependencyObservable ??= new DependencyObservable(this)));

            return new ReactiveVersionedValue<TResult>(this.GetFunctionResult(), this._version);
        }

        protected void EnsureFunctionEvaluated()
        {
            if (this._isFunctionResultDirty)
                // This lock will avoid concurrent evaluations and evaluations concurrent to updates.
            {
                lock (this.Sync)
                {
                    if (this._isFunctionResultDirty)
                    {
                        // Evaluate the source.
                        var input = this.Source.GetVersionedValue(this.CollectorToken);

                        if (input.Version != this._inputVersion || !this._dependencies.IsDirty())
                        {
                            this._inputVersion = input.Version;

                            this._dependencies.Clear();

                            // If the source has changed, we need to evaluate our function again.
                            var isDifferent = this.EvaluateFunction(input.Value);

                            if (this._version < 0 || isDifferent)
                                // Our function gave a different result, so we increase our version number.
                            {
                                this._version++;
                            }
                        }

                        this._isFunctionResultDirty = false;
                    }

                    this.EnsureSubscribedToSource();
                }
            }
        }


        
        private void OnValueChanged(bool isBreakingChange)
        {
            if (isBreakingChange)
            {
                this.OnObserverBreakingChange();
            }
        }

        protected void OnObserverBreakingChange()
        {
            this._isFunctionResultDirty = true;

            foreach (var subscription in this.Observers.WeaklyTyped())
            {
                subscription.Observer.OnValueInvalidated(subscription.Subscription, true);
            }
        }

        protected virtual void OnOtherValueInvalidated(IReactiveSubscription subscription, bool isBreakingChange)
        {
            this.OnObserverBreakingChange();
        }


        protected readonly struct UpdateToken : IDisposable
        {
            private readonly ReactiveOperator<TSource, TSourceObserver, TResult, TResultObserver> _parent;

            public UpdateToken(ReactiveOperator<TSource, TSourceObserver, TResult, TResultObserver> parent)
            {
                Monitor.Enter(parent.Sync);

                this._parent = parent;

                Debug.Assert(!parent._currentUpdateHasChange);
            }

            
            public void SignalChange(bool isBreakingCachedFunctionResult = false)
            {
                if (this._parent == null)
                    throw new InvalidOperationException();


                if (!this._parent._currentUpdateHasChange)
                {
                    this._parent._currentUpdateHasChange = true;
                    this._parent._version++;
                }

                if (isBreakingCachedFunctionResult && !this._parent._isFunctionResultDirty )
                {
                    
                    // We have an incremental change that breaks the stored value, so _parent.EvaluateFunction() must
                    // be called again. However, observers don't need to resynchronize if they are able to process
                    // the increment.
                    
                    this._parent._isFunctionResultDirty = true;
                    
                    // We don't nullify the old result now because the current result may eventually be still valid if all versions 
                    // end up being identical.
                }

            }

            public int Version => this._parent._version;


            public void Dispose()
            {
                if (this._parent == null)
                {
                    return;
                }


                if (this._parent._currentUpdateHasChange)
                {
                    this._parent._currentUpdateHasChange = false;

                    foreach (var subscription in this._parent.Observers.WeaklyTyped())
                    {
                        subscription.Observer.OnValueInvalidated(subscription.Subscription, false);
                    }

                }

                Monitor.Exit(this._parent.Sync);
            }
        }

       

        class DependencyObservable : IReactiveObservable<IReactiveObserver>
        {
            private readonly ReactiveOperator<TSource, TSourceObserver, TResult, TResultObserver> _parent;

            public DependencyObservable(ReactiveOperator<TSource, TSourceObserver, TResult, TResultObserver> parent)
            {
                this._parent = parent;
            }

            public object Object => this._parent;

            IReactiveSubscription IReactiveObservable<IReactiveObserver>.AddObserver(IReactiveObserver observer)
            {
                return this._parent.Observers.AddObserver(observer);
            }

            bool IReactiveObservable<IReactiveObserver>.RemoveObserver(IReactiveSubscription subscription)
            {
                return this._parent.Observers.RemoveObserver(subscription);
            }

            public bool HasPathToObserver(object observer)
            {
                return observer == this._parent || this._parent.Observers.HasPathToObserver(observer);
            }

            bool IReactiveSource.IsMaterialized => this._parent.IsMaterialized;

            bool IReactiveSource.IsImmutable => this._parent.IsImmutable;

            int IReactiveSource.Version => this._parent._version;
        }
    }
}