// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using Metalama.Reactive.Implementation;

namespace Metalama.Reactive.Operators
{
    internal class WriteLineOperator<T> : IReactiveCollectionObserver<T>, IReactiveCollector
    {
        private readonly IReactiveCollection<T> _source;
        private readonly IReactiveSubscription? _subscription;
        private DependencyList _dependencies;

        public WriteLineOperator( IReactiveCollection<T> source, string? name )
        {
            this._source = source;
            this.Name = name ?? typeof( T ).Name;
            this._subscription = this._source.Observable.AddObserver( this );
            this._dependencies = new DependencyList( this );

            foreach ( var item in this._source.GetValue( new ReactiveCollectorToken( this ) ) )
            {
                this.Add( item );
            }
        }

        private void Add( T item ) => Console.WriteLine( $"{this.Name}: Added {item}" );

        private void Remove( T item ) => Console.WriteLine( $"{this.Name}: Removed {item}" );

        public string Name { get; }

        public void Dispose()
        {
            this._dependencies.Clear();
            this._subscription?.Dispose();
        }

        void IReactiveCollectionObserver<T>.OnItemAdded( IReactiveSubscription subscription, T item, int newVersion )
        {
            this.Add( item );
        }

        void IReactiveCollectionObserver<T>.OnItemRemoved( IReactiveSubscription subscription, T item, int newVersion )
        {
            this.Remove( item );
        }

        void IReactiveCollectionObserver<T>.OnItemReplaced( IReactiveSubscription subscription, T oldItem, T newItem, int newVersion )
        {
            Console.WriteLine( $"{this.Name}: Replaced {oldItem}  ->  {newItem}." );
        }

        void IReactiveObserver.OnValueInvalidated( IReactiveSubscription subscription, bool isBreakingChange )
        {
            if ( isBreakingChange )
            {
                foreach ( var item in this._source.GetValue( new ReactiveCollectorToken( this ) ) )
                {
                    this.Add( item );
                }
            }
        }

        void IReactiveObserver<IEnumerable<T>>.OnValueChanged(
            IReactiveSubscription subscription,
            IEnumerable<T> oldValue,
            IEnumerable<T> newValue,
            int newVersion,
            bool isBreakingChange )
        {
            if ( isBreakingChange )
            {
                foreach ( var item in oldValue )
                {
                    this.Remove( item );
                }

                foreach ( var item in newValue )
                {
                    this.Add( item );
                }
            }
        }

        void IReactiveCollector.AddDependency( IReactiveObservable<IReactiveObserver> source, int version )
        {
            if ( source.Source != this._source )
            {
                this._dependencies.Add( source, version );
            }
        }

        void IReactiveCollector.AddSideValue( IReactiveSideValue value )
        {
        }

        void IReactiveCollector.AddSideValues( ReactiveSideValues values )
        {
        }
    }
}