// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Metalama.Reactive.Implementation
{
    public readonly struct ReactiveOperatorResult<TResult>
    {
        public TResult Value { get; }

        public ReactiveSideValues SideValues { get; }

        public ReactiveOperatorResult( TResult value, ReactiveSideValues sideValues = default )
        {
            this.Value = value;
            this.SideValues = sideValues;
        }

        public static implicit operator ReactiveOperatorResult<TResult>( TResult result ) => new ReactiveOperatorResult<TResult>( result );
    }
}