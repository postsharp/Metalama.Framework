// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Advising;
using Metalama.Framework.Code;
using System;

namespace Metalama.Framework.Engine.Advising;

internal class IntroduceConstructorParameterAdviceResult : AdviceResult, IIntroductionAdviceResult<IParameter>
{
    private readonly IRef<IParameter>? _declaration;

    public IParameter Declaration => this.Resolve( this._declaration );

    public IDeclaration ConflictingDeclaration => throw new NotSupportedException();

    public IntroduceConstructorParameterAdviceResult() { }

    public IntroduceConstructorParameterAdviceResult( IRef<IParameter>? declaration )
    {
        this.AdviceKind = AdviceKind.IntroduceParameter;
        this._declaration = declaration;
    }
}