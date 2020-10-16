#region

using System;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

#endregion

namespace Caravela.Reactive
{
    internal abstract class ReactiveOperator<TSource, TSourceObserver, TResult, TResultObserver> :
        IReactiveSource<TResult, TResultObserver>,
        IReactiveObserver<TSource>,
        IReactiveTokenCollector
        where TSourceObserver : IReactiveObserver<TSource>
        where TResultObserver : class, IReactiveObserver<TResult>

    {
        private DependencyList _dependencies;
        private bool _currentUpdateHasChange;
        private volatile int _inputVersion = -1;
        private volatile bool _isDirty = true;
        private volatile int _version = -1;
        private DependencyObservable _dependencyObservable;

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


        private IReactiveSubscription SubscriptionToSource { get; set; }


        protected IReactiveSource<TSource, TSourceObserver> Source { get; }

        protected bool MustProcessInboundChange => this._version >= 0;

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

            return this.Observers.AddObserver(observer);
        }

        bool IReactiveObservable<TResultObserver>.RemoveObserver(IReactiveSubscription subscription)
        {
            return this.Observers.RemoveObserver(subscription);
        }

        bool IReactiveDebugging.HasPathToObserver(object observer)
        {
            return observer == this || this.Observers.HasPathToObserver(observer);
        }

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

        protected internal virtual void UnsubscribeFromSource()
        {
            this.SubscriptionToSource?.Dispose();
            this.SubscriptionToSource = default;
        }

        protected abstract bool EvaluateFunction(TSource source);

        protected bool CanProcessIncrementalChange => !this._isDirty;


        protected UpdateToken GetIncrementalUpdateToken()
        {
            return new UpdateToken(this);
        }


        private ReactiveVersionedValue<TResult> GetValueImpl(in ReactiveCollectorToken collectorToken)
        {
            collectorToken.Collector?.AddDependency((this._dependencyObservable ??= new DependencyObservable(this)));

            this.EnsureFunctionEvaluated();

            return new ReactiveVersionedValue<TResult>(this.GetFunctionResult(), this._version);
        }

        protected void EnsureFunctionEvaluated()
        {
            if (this._isDirty)
                // This lock will avoid concurrent evaluations and evaluations concurrent to updates.
            {
                lock (this.Sync)
                {
                    if (this._isDirty)
                    {
                        // Evaluate the source.
                        var input = this.Source.GetVersionedValue(this.CollectorToken);

                        if (input.Version != this._inputVersion || !this._dependencies.IsEmpty)
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

                        this._isDirty = false;
                    }

                    this.EnsureSubscribedToSource();
                }
            }
        }


        protected abstract TResult GetFunctionResult();

        private void OnValueChanged(bool isBreakingChange)
        {
            if (isBreakingChange)
            {
                this.OnBreakingChange();
            }
        }

        protected void OnBreakingChange()
        {
            if (_isDirty)
                return;
            
            this._isDirty = true;

            foreach (var subscription in this.Observers.WeaklyTyped())
            {
                subscription.Observer.OnValueInvalidated(subscription.Subscription, true);
            }
        }

        protected virtual void OnOtherValueInvalidated(IReactiveSubscription subscription, bool isBreakingChange)
        {
            this.OnBreakingChange();
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

            
            public void SignalChange()
            {
                if (this._parent == null)
                    throw new InvalidOperationException();

                if (this._parent._isDirty)
                    throw new InvalidOperationException();

                if (!this._parent._currentUpdateHasChange)
                {
                    this._parent._currentUpdateHasChange = true;
                    this._parent._version++;
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
        }
    }
}