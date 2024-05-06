// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Advising;
using Metalama.Framework.Code;
using System;

namespace Metalama.Framework.Engine.Advising;

internal sealed partial class AdviceFactory<T> where T : IDeclaration
{
    private class TypeIntroductionAdviceResult : ITypeIntroductionAdviceResult
    {
        private readonly IIntroductionAdviceResult<INamedType> _inner;

        public TypeIntroductionAdviceResult( IIntroductionAdviceResult<INamedType> inner )
        {
            this._inner = inner;
        }

        public INamedType Declaration => this._inner.Declaration;

        public IDeclaration ConflictingDeclaration => this._inner.ConflictingDeclaration;

        public AdviceKind AdviceKind => this._inner.AdviceKind;

        public AdviceOutcome Outcome => this._inner.Outcome;

        public INamedType Target => this._inner.Declaration;

        public IAdviser<TNewDeclaration> WithTarget<TNewDeclaration>( TNewDeclaration target ) where TNewDeclaration : IDeclaration
            => throw new NotImplementedException();
    }
}