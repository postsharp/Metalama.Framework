#region

using Caravela.Reactive.Implementation;
using System;
using System.Collections.Generic;
using System.Threading;

#endregion

namespace Caravela.Reactive.Sources
{
    public sealed class ReactiveSource<T> : IReactiveSource<T, IReactiveObserver<T>>, IReactiveObservable<IReactiveObserver<T>>
    {
        private int _version;
        private readonly IEqualityComparer<T> _comparer;
        private IReactiveVersionedValue<T> _value;
        private ObserverList<IReactiveObserver<T>> _observers;

        public ReactiveSource( T value, IEqualityComparer<T>? comparer = null )
        {
            this._value = new ReactiveVersionedValue<T>( value, 0 );
            this._comparer = comparer ?? EqualityComparer<T>.Default;
            this._observers = new ObserverList<IReactiveObserver<T>>( this );
        }

        public T Value
        {
            get => this._value.Value;
            set
            {
                if ( !this._comparer.Equals( value, this._value.Value ) )
                {
                    int version = Interlocked.Increment( ref this._version );
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