﻿using Caravela.Reactive.Implementation;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace Caravela.Reactive.Operators
{
    class SelectManyFromScalarOperator<TSource, TResult> : ReactiveOperator<TSource, IReactiveObserver<TSource>, IEnumerable<TResult>, IReactiveCollectionObserver<TResult>>,
        IReactiveCollection<TResult>
    {
        IEnumerable<TResult>? _cachedResult;
        readonly Func<TSource, ReactiveCollectorToken, IImmutableList<TResult>> _func;

        public SelectManyFromScalarOperator( IReactiveSource<TSource, IReactiveObserver<TSource>> source, Func<TSource, IImmutableList<TResult>> func ) : base( source )
        {
            this._func = ReactiveCollectorToken.WrapWithDefaultToken( func );
        }

        protected override ReactiveOperatorResult<IEnumerable<TResult>> EvaluateFunction( TSource source )
        {
            this._cachedResult = this._func( source, this.ObserverToken );
            return new( this._cachedResult );
        }

        protected override IReactiveSubscription? SubscribeToSource() => this.Source.Observable.AddObserver( this );

        protected override void OnSourceValueChanged( bool isBreakingChange )
        {
            var newResult = this._func( this.Source.GetValue(this.ObserverToken), this.ObserverToken );

            this.OnSourceValueChanged( newResult );
        }

        protected override void OnSourceValueChanged( bool isBreakingChange, TSource oldValue, TSource newValue )
        {
            
            var newResult = this._func( newValue, this.ObserverToken );

            this.OnSourceValueChanged( newResult );
        }

        private void OnSourceValueChanged( IEnumerable<TResult> newResult )
        {
            if ( !ReferenceEquals( this._cachedResult, newResult ) )
            {
                // TODO: not sure if side values are handled properly.

                using var tk = this.GetIncrementalUpdateToken();
                this._cachedResult = newResult;
                tk.SetValue( newResult );
            }
        }
    }
}
