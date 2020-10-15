using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Caravela.Reactive
{
    internal abstract class ReactiveOperator<TSource, TSourceObserver, TResult, TResultObserver> :
        IReactiveSource<TResult, TResultObserver>, IReactiveObserver<TSource>,
        IReactiveTokenCollector where TSourceObserver : IReactiveObserver<TSource>
        where TResultObserver : IReactiveObserver<TResult>

    {
        private readonly Dictionary<object, IReactiveSubscription> _dependencies =
            new Dictionary<object, IReactiveSubscription>();

        private bool _currentUpdateHasChange;
        private bool _hasBreakingChange; // Has any change that breaks incremental reconstruction.
        private volatile int _inputVersion = -1;
        private volatile bool _isDirty;

        private volatile int _version = -1;

        protected ReactiveOperator(IReactiveSource<TSource, TSourceObserver> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            Source = source;
            Observers = new ObserverList<TResultObserver>(this);
        }

        protected ObserverList<TResultObserver> Observers { get; }
        private object Sync => Observers;


        private IReactiveSubscription? SubscriptionToSource { get; set; }

        protected IReactiveSource<TSource, TSourceObserver> Source { get; }
        protected bool HasObserver => !Observers.IsEmpty;

        protected bool MustProcessInboundChange => _version >= 0;

        protected ReactiveCollectorToken CollectorToken => new ReactiveCollectorToken(this);

        void IReactiveObserver.OnValueInvalidated(IReactiveSubscription subscription, bool isBreakingChange)
        {
            if (subscription == SubscriptionToSource)
                OnValueChanged(isBreakingChange);
            else
                OnOtherValueInvalidated(subscription, isBreakingChange);
        }


        void IReactiveObserver<TSource>.OnValueChanged(IReactiveSubscription subscription, TSource oldValue,
            TSource newValue, int newVersion, bool isBreakingChange)
        {
            OnValueChanged(isBreakingChange);
        }

        public void Dispose()
        {
            UnsubscribeFromSource();
        }


        IReactiveSubscription IReactiveObservable<TResultObserver>.AddObserver(TResultObserver observer)
        {
            if (SubscriptionToSource == null) SubscriptionToSource = SubscribeToSource();

            return Observers.AddObserver(observer);
        }


        bool IReactiveObservable<TResultObserver>.RemoveObserver(IReactiveSubscription subscription)
        {
            return Observers.RemoveObserver(subscription);
        }

        public TResult GetValue(in ReactiveCollectorToken collectorToken)
        {
            return GetValueImpl().Value;
        }

        IReactiveVersionedValue<TResult> IReactiveSource<TResult, TResultObserver>.
            GetVersionedValue(in ReactiveCollectorToken collectorToken)
        {
            return GetValueImpl();
        }

        public virtual bool IsMaterialized => false;

        void IReactiveTokenCollector.AddDependency(IReactiveObservable<IReactiveObserver> observable)
        {
            if (observable != this)
                lock (_dependencies)
                {
                    if (!_dependencies.ContainsKey(observable))
                        _dependencies.Add(observable, observable.AddObserver(this));
                }
        }

        protected internal abstract IReactiveSubscription SubscribeToSource();

        protected internal virtual void UnsubscribeFromSource()
        {
            SubscriptionToSource?.Dispose();
            SubscriptionToSource = default;
        }

        protected abstract bool EvaluateFunction(TSource source);


        protected UpdateToken GetUpdateToken()
        {
            return new UpdateToken(this);
        }


        private ReactiveVersionedValue<TResult> GetValueImpl()
        {
            EnsureFunctionEvaluated();

            return new ReactiveVersionedValue<TResult>(GetFunctionResult(), _version);
        }

        protected void EnsureFunctionEvaluated()
        {
            if (_isDirty)
                // This lock will avoid concurrent evaluations and evaluations concurrent to updates.
                lock (Sync)
                {
                    if (_isDirty)
                    {
                        // Evaluate the source.
                        var input = Source.GetVersionedValue(CollectorToken);

                        if (input.Version != _inputVersion)
                        {
                            _inputVersion = input.Version;

                            UnfollowAllDependencies();

                            // If the source has changed, we need to evaluate our function again.
                            var isDifferent = EvaluateFunction(input.Value);

                            if (_version < 0 || isDifferent)
                                // Our function gave a different result, so we increase our version number.
                                _version++;
                        }

                        _isDirty = false;
                    }

                    SubscriptionToSource ??= SubscribeToSource();
                }
        }


        protected abstract TResult GetFunctionResult();

        private void OnValueChanged(bool isIncremental)
        {
            using (var token = GetUpdateToken())
            {
                token.SignalChange(!isIncremental);
            }
        }

        protected virtual void OnOtherValueInvalidated(IReactiveSubscription subscription, bool isBreakingChange)
        {
            throw new InvalidOperationException();
        }


        private void UnfollowAllDependencies()
        {
            // Dispose previous dependencies.
            lock (_dependencies)
            {
                foreach (var subscription in _dependencies.Values) subscription.Dispose();
            }
        }


        protected readonly struct UpdateToken : IDisposable
        {
            private readonly ReactiveOperator<TSource, TSourceObserver, TResult, TResultObserver> _parent;

            public UpdateToken(ReactiveOperator<TSource, TSourceObserver, TResult, TResultObserver> parent)
            {
                Monitor.Enter(parent.Sync);

                _parent = parent;

                Debug.Assert(!parent._currentUpdateHasChange);
            }

            public void SignalChange(bool isBreakingChange = false)
            {
                if (!_parent._currentUpdateHasChange)
                {
                    _parent._isDirty = true;
                    _parent._currentUpdateHasChange = false;
                    _parent._version++;
                }

                if (isBreakingChange) _parent._hasBreakingChange = true;
            }

            public int Version => _parent._version;


            public void Dispose()
            {
                if (_parent == null)
                    return;


                if (_parent._currentUpdateHasChange)
                    foreach (var subscription in _parent.Observers)
                        subscription.Observer.OnValueInvalidated(subscription, _parent._hasBreakingChange);
                else
                    _parent._currentUpdateHasChange = false;

                Monitor.Exit(_parent.Sync);
            }
        }
    }
}