#region

using System;
using System.Collections.Generic;

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
    public abstract partial class BaseReactiveOperator<TSource, TSourceObserver, TResult, TResultObserver> :
        IReactiveSource,
    IReactiveObserver<TSource>,
    IReactiveObservable<TResultObserver>,
    IReactiveCollector
    where TSourceObserver : class, IReactiveObserver<TSource>
    where TResultObserver : class, IReactiveObserver<TResult>

    {

        private protected DependencyList _dependencies;
        private protected volatile int _sourceVersion = -1;
        private protected volatile bool _isFunctionResultDirty = true;
        private protected AtomicValue<ReactiveVersionedValue<TResult>> _result;
        private DependencyObservable? _dependencyObservable;
        private protected IReactiveSubscription? _subscriptionToSource;
        private ObserverList<TResultObserver> _observers;

        // The following fields logically belong to IncrementalUpdateToken but this
        // type needs to be immutable so that we can use it with the 'using' statement.
        // Since there is one or zero update concurrently, we're storing the state here.
        private protected IncrementalUpdateStatus _currentUpdateStatus;
        private bool _currentUpdateBreaksObservers;
        private int _currentUpdateNewSourceVersion;
        private TResult? _currentUpdateResult;
        private ReactiveSideValues _currentUpdateSideValues;


        public abstract bool IsImmutable { get; }

        protected ObserverListEnumerator<TResultObserver, TResultObserver> Observers => this._observers.GetEnumerator();

        protected IReactiveSource Source { get; }


        protected ReactiveCollectorToken ObserverToken => new ReactiveCollectorToken( this );

        protected TResult CachedValue => this._result.Value.Value;


        void IReactiveObserver.OnValueInvalidated( IReactiveSubscription subscription, bool isBreakingChange )
        {
            if ( subscription == this._subscriptionToSource )
            {
                this.OnSourceValueChanged( isBreakingChange );
            }
            else
            {
                this.OnObserverBreakingChange();
            }
        }


        void IReactiveObserver<TSource>.OnValueChanged( IReactiveSubscription subscription, TSource oldValue,
            TSource newValue, int newVersion, bool isBreakingChange )
        {
            this.OnSourceValueChanged( isBreakingChange, oldValue, newValue );
        }

        public void Dispose()
        {
            this._dependencies.Clear();
            this.UnsubscribeFromSource();
        }


        IReactiveSubscription? IReactiveObservable<TResultObserver>.AddObserver( TResultObserver observer )
        {
            this.EnsureSubscribedToSource();

            if ( this.IsImmutable )
            {
                return null;
            }

            return this._observers.AddObserver( observer );
        }

        bool IReactiveObservable<TResultObserver>.RemoveObserver( IReactiveSubscription? subscription )
        {
            if ( subscription == null )
            {
                return false;
            }

            return this._observers.RemoveObserver( subscription );
        }

      

        public int Version => this._result.Value.Version;


        public virtual bool IsMaterialized => false;

        IReactiveSource IReactiveObservable<TResultObserver>.Source => this;

        protected virtual bool ShouldTrackDependency( IReactiveObservable<IReactiveObserver> source )
            => source.Source != this && source.Source != this.Source;

        void IReactiveCollector.AddDependency( IReactiveObservable<IReactiveObserver> source, int version )
        {
            if ( this.ShouldTrackDependency( source ) && !this.IsImmutable )
            {
                this._dependencies.Add( source, version );
            }
        }
        void IReactiveCollector.AddSideValue( IReactiveSideValue? value )
        {
            if ( value != null )
            {
                this._currentUpdateSideValues = this._currentUpdateSideValues.Combine( value );
            }
        }

        void IReactiveCollector.AddSideValues( ReactiveSideValues value )
        {
            this._currentUpdateSideValues = this._currentUpdateSideValues.Combine( value );
        }

        protected abstract IReactiveSubscription? SubscribeToSource();

        protected void EnsureSubscribedToSource()
        {
            if ( this._subscriptionToSource == null )
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
        /// Determines whether two return values of <see cref="ReactiveOperator{TSource, TSourceObserver, TResult, TResultObserver}.EvaluateFunction"/> are equal.
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        protected virtual bool AreEqual( TResult first, TResult second ) =>
            EqualityComparer<TResult>.Default.Equals( first, second );


        /// <summary>
        /// Determines whether the current operator should process incremental changes.
        /// This is  useless only if the operator has not yet been evaluated. It can be useful even if
        /// the current operator has a breaking change because the operators downstream may still be
        /// able to process incremental changes.
        /// </summary>
        protected bool ShouldProcessIncrementalChange => this._result.Value.Version > 0;


     

        private protected void CollectDependencies( ReactiveCollectorToken observerToken, int version )
        {
            // Collect after evaluation so that the version number is updated.
            observerToken.Collector?.AddDependency( (this._dependencyObservable ??= new DependencyObservable( this )), version );
        }


        protected virtual void OnSourceValueChanged( bool isBreakingChange )
        {
            if ( isBreakingChange )
            {
                this.OnObserverBreakingChange();
            }
        }

        protected virtual void OnSourceValueChanged( bool isBreakingChange, TSource oldValue, TSource newValue )
        {
            if ( isBreakingChange )
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

            foreach ( var subscription in this._observers.WeaklyTyped() )
            {
                subscription.Observer.OnValueInvalidated( subscription.Subscription, true );
            }
        }

     

        protected BaseReactiveOperator( IReactiveSource source)
        {
            this.Source = source ?? throw new ArgumentNullException(nameof(source));
            this._observers = new ObserverList<TResultObserver>(this);
            this._dependencies = new DependencyList(this);
        }



    }
}