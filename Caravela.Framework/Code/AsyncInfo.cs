// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Caravela.Framework.Code
{
    public readonly struct AsyncInfo
    {
        public bool IsAsync { get; }

        public bool IsAwaitable { get; }

        public bool IsAwaitableOrVoid => this.IsAwaitable || this.ResultType.Is( SpecialType.Void );

        public IType ResultType { get; }

        internal AsyncInfo( bool isAsync, bool isAwaitable, IType resultType )
        {
            this.IsAsync = isAsync;
            this.IsAwaitable = isAwaitable;
            this.ResultType = resultType;
        }
    }
}