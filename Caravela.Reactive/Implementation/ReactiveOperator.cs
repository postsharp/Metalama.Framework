#region

using System;
using System.Collections.Generic;
using System.Threading;

#endregion

namespace Caravela.Reactive.Implementation
{
    /// <summary>
    /// A base implementation of a reactive operation, i.e. a function that maps an input to an output
    /// and reacts to changes of the input by invalidating the output and/or processing incremental
    /// change notifications. 
    /// </summary>
    /// <typeparam name="TSource">Type of the source value.</typeparam>
    /// <typeparam name="TSourceObserver">Type of source observers.</typeparam>
    /// <typeparam name="TResult">Type of the result value.</typeparam>
    /// <typeparam name="TResultObserver">Type of result observers.</typeparam>
    public abstract partial class ReactiveOperator<TSource, TSourceObserver, TResult, TResultObserver> :
        IReactiveSource<TResult, TResultObserver>,
        IReactiveObserver<TSource>,
        IReactiveTokenCollector
        where TSourceObserver : class, IReactiveObserver<TSource>
        where TResultObserver : class, IReactiveObserver<TResult>

    {
        private DependencyList _dependencies;

        private volatile int _sourceVersion = -1;
        private volatile bool _isFunctionResultDirty = true;
        private volatile ReactiveVersionedValue<TResult>? _result;
        private DependencyObservable? _dependencyObservable;
        private IReactiveSubscription? _subscriptionToSource;
        private SpinLock _lock = default;
        private ObserverList<TResultObserver> _observers;

        // The following fields logically belong to IncrementalUpdateToken but this
        // type needs to be immutable so that we can use it with the 'using' statement.
        // Since there is one or zero update concurrently, we're storing the state here.
        private IncrementalUpdateStatus _currentUpdateStatus;
        private int _currentUpdateNewSourceVersion;
        private TResult? _currentUpdateResult;


        protected ReactiveOperator(IReactiveSource<TSource, TSourceObserver> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            this.Source = source;
            this._observers = new ObserverList<TResultObserver>(this);
            this._dependencies = new DependencyList(this);
        }

        protected ObserverListEnumerator<TResultObserver, TResultObserver> Observers => this._observers.GetEnumerator();
        


        protected IReactiveSource<TSource, TSourceObserver> Source { get; }


        protected ReactiveObserverToken ObserverToken => new ReactiveObserverToken(this);

        protected TResult CachedValue => this._result!.Value;


        void IReactiveObserver.OnValueInvalidated(IReactiveSubscription subscription, bool isBreakingChange)
        {
            if (subscription == this._subscriptionToSource)
            {
                this.OnValueChanged(isBreakingChange);
            }
            else
            {
                this.OnObserverBreakingChange();
            }
        }


        void IReactiveObserver<TSource>.OnValueChanged(IReactiveSubscription subscription, TSource oldValue,
            TSource newValue, int newVersion, bool isBreakingChange)
        {
            this.OnValueChanged(isBreakingChange);
        }

        public void Dispose()
        {
            this._dependencies.Clear();
            this.UnsubscribeFromSource();
        }


        IReactiveSubscription? IReactiveObservable<TResultObserver>.AddObserver(TResultObserver observer)
        {
            this.EnsureSubscribedToSource();

            if (this.IsImmutable)
            {
                return null;
            }

            return this._observers.AddObserver(observer);
        }

        bool IReactiveObservable<TResultObserver>.RemoveObserver(IReactiveSubscription subscription)
        {
            if (subscription == null)
            {
                return false;
            }

            return this._observers.RemoveObserver(subscription);
        }

        public bool IsImmutable
        {
            get
            {
                if ( !this.Source.IsImmutable )
                {
                    return false;
                }

                if ( this._result == null )
                {
                    // The function has never been evaluated, so dependencies were not collected.
                    this.EnsureFunctionEvaluated();
                }


                return this._dependencies.IsEmpty;
            }
        }

        public int Version => this._result?.Version ?? -1;

        public TResult GetValue(in ReactiveObserverToken observerToken = default) =>
            this.GetVersionedValue(observerToken).Value;


        public ReactiveVersionedValue<TResult> GetVersionedValue(in ReactiveObserverToken observerToken = default)
        {
            this.EnsureFunctionEvaluated();

            // Take local copy of the result to guarantee consistency in the version number.
            var currentValue = this._result;
            this.CollectDependencies(observerToken, currentValue!.Version);

            return this._result!;
        }


        IReactiveVersionedValue<TResult> IReactiveSource<TResult>.GetVersionedValue(
            in ReactiveObserverToken observerToken)
        {
            return this.GetVersionedValue(observerToken);
        }

        public virtual bool IsMaterialized => false;

        object IReactiveObservable<TResultObserver>.Object => this;

        protected virtual bool ShouldTrackDependency(IReactiveObservable<IReactiveObserver> source)
            => source.Object != this && source.Object != this.Source;

        void IReactiveTokenCollector.AddDependency(IReactiveObservable<IReactiveObserver> source, int version)
        {
            if (this.ShouldTrackDependency(source) && !this.IsImmutable)
            {
                this._dependencies.Add(source, version);
            }
        }

        protected abstract IReactiveSubscription? SubscribeToSource();

        protected internal void EnsureSubscribedToSource()
        {
            if (this._subscriptionToSource == null)
            {
                this._subscriptionToSource = this.SubscribeToSource();
            }
        }

        protected virtual void UnsubscribeFromSource()
        {
            this._subscriptionToSource?.Dispose();
            this._subscriptionToSource = default;
        }

        /// <summary>
        /// Evaluates the function (i.e. the operator) and returns its value.
        /// </summary>
        /// <param name="source">Source value.</param>
        /// <returns>Result value.</returns>
        protected abstract TResult EvaluateFunction(TSource source);

        /// <summary>
        /// Determines whether two return values of <see cref="EvaluateFunction"/> are equal.
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        protected virtual bool AreEqual(TResult first, TResult second) =>
            EqualityComparer<TResult>.Default.Equals(first, second);


        /// <summary>
        /// Determines whether the current operator should process incremental changes.
        /// This is  useless only if the operator has not yet been evaluated. It can be useful even if
        /// the current operator has a breaking change because the operators downstream may still be
        /// able to process incremental changes.
        /// </summary>
        protected bool ShouldProcessIncrementalChange => this._result != null;


        /// <summary>
        /// Gets an <see cref="IncrementalUpdateToken"/>, which allows to represent incremental changes.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        protected IncrementalUpdateToken GetIncrementalUpdateToken(int sourceVersion = -1)
        {
            if (this._currentUpdateStatus != IncrementalUpdateStatus.Default && this._currentUpdateStatus != IncrementalUpdateStatus.Disposed)
            {
                throw new InvalidOperationException();
            }

            return new IncrementalUpdateToken(this, sourceVersion);
        }


        private void CollectDependencies(ReactiveObserverToken observerToken, int version)
        {
            // Collect after evaluation so that the version number is updated.
            observerToken.Collector?.AddDependency((this._dependencyObservable ??= new DependencyObservable(this)), version);
        }

        /// <summary>
        /// Evaluates the operator if necessary.
        /// </summary>
        protected void EnsureFunctionEvaluated()
        {
            if (this._isFunctionResultDirty)
                // This lock will avoid concurrent evaluations and evaluations concurrent to updates.
            {
                var lockHeld = false;
                try
                {
                    this._lock.Enter(ref lockHeld);

                    if (this._isFunctionResultDirty)
                    {
                        // Evaluate the source.
                        var input = this.Source.GetVersionedValue(this.ObserverToken);

                        if (input.Version != this._sourceVersion || !this._dependencies.IsDirty())
                        {
                            this._sourceVersion = input.Version;

                            this._dependencies.Clear();

                            // If the source has changed, we need to evaluate our function again.
                            var newResult = this.EvaluateFunction(input.Value);

                            if (this._result == null || !this.AreEqual(this._result.Value, newResult))
                            {
                                // Our function gave a different result, so we increase our version number.
                                this._result =
                                    new ReactiveVersionedValue<TResult>(newResult, this._result?.Version + 1 ?? 0);
                            }

                            // If the function has not produced dependencies on first execution, we forbid to product later.
                            // This makes sure we can implement the IsImmutable property reliably.
                            if ( this._dependencies.IsEmpty )
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
                    if (lockHeld)
                    {
                        this._lock.Exit();
                    }
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

        /// <summary>
        /// Method called when there are changes in the current operator that break the ability
        /// of observers to process incremental changes. Observers must then reevaluate the whole operator.
        /// </summary>
        protected void OnObserverBreakingChange()
        {
            this._isFunctionResultDirty = true;

            foreach (var subscription in this._observers.WeaklyTyped())
            {
                subscription.Observer.OnValueInvalidated(subscription.Subscription, true);
            }
        }

    }
}