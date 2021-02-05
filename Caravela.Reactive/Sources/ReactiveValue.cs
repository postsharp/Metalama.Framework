using System;
using System.Collections.Generic;
using System.Threading;
using Caravela.Reactive.Implementation;

namespace Caravela.Reactive.Sources
{
    /// <summary>
    /// An implementation of <see cref="IReactiveSource{T}"/> that has a single value exposed on a writable
    /// <see cref="Value"/> property. Modifying the value will raise the appropriate events to the
    /// observers.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class ReactiveValue<T> : IReactiveSource<T, IReactiveObserver<T>>, IReactiveObservable<IReactiveObserver<T>>
    {
        private IReactiveVersionedValue<T> _value;
        private int _version;
        private ObserverList<IReactiveObserver<T>> _observers;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReactiveValue{T}"/> class and sets the initial value.
        /// </summary>
        /// <param name="value">The initial value.</param>
        /// <param name="comparer">A value comparer, to determine when the value has changed.</param>
        public ReactiveValue( T value, IEqualityComparer<T>? comparer = null )
        {
            this._value = new ReactiveVersionedValue<T>( value, 0 );
            this._comparer = comparer ?? EqualityComparer<T>.Default;
            this._observers = new ObserverList<IReactiveObserver<T>>( this );
        }

        /// <summary>
        /// Gets or sets the value encapsulated by the current object. Setting the value to a different value
        /// will raise the appropriate event to all registered observers.
        /// </summary>
        public T Value
        {
            get => this._value.Value;
            set
            {
                if ( !this._comparer.Equals( value, this._value.Value ) )
                {
                    var version = Interlocked.Increment( ref this._version );
                    var oldValue = this._value;

                    this._value = new ReactiveVersionedValue<T>( value, this._version );

                    // Notify observers.
                    foreach ( var observer in this._observers )
                    {
                        observer.Observer.OnValueChanged( observer.Subscription, oldValue.Value, value, version );
                    }

                    foreach ( var observer in this._observers.WeaklyTyped() )
                    {
                        observer.Observer.OnValueInvalidated( observer.Subscription, false );
                    }
                }
            }
        }

        int IReactiveObservable<IReactiveObserver<T>>.Version => throw new NotImplementedException();

        IReactiveSource IReactiveObservable<IReactiveObserver<T>>.Source => this;

        IReactiveObservable<IReactiveObserver<T>> IReactiveSource<T, IReactiveObserver<T>>.Observable => this;

        bool IReactiveSource.IsMaterialized => true;

        bool IReactiveSource.IsImmutable => false;

        IReactiveSubscription? IReactiveObservable<IReactiveObserver<T>>.AddObserver( IReactiveObserver<T> observer )
        {
            return this._observers.AddObserver( observer );
        }

        bool IReactiveObservable<IReactiveObserver<T>>.RemoveObserver( IReactiveSubscription subscription )
        {
            return this._observers.RemoveObserver( subscription );
        }

        T IReactiveSource<T>.GetValue( in ReactiveCollectorToken observerToken ) => this._value.Value;

        IReactiveVersionedValue<T> IReactiveSource<T>.GetVersionedValue( in ReactiveCollectorToken observerToken ) => this._value;
    }
}