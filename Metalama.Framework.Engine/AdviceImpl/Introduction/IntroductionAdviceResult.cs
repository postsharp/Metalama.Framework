// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Advising;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.Utilities;

namespace Metalama.Framework.Engine.AdviceImpl.Introduction;

internal sealed class IntroductionAdviceResult<T> : AdviceResult, IIntroductionAdviceResult<T>
    where T : class, IDeclaration
{
    private readonly IRef<T>? _declaration;
    private readonly IRef<IDeclaration>? _conflictingDeclaration;

    public IntroductionAdviceResult(
        AdviceKind adviceKind,
        AdviceOutcome outcome,
        IRef<T>? declaration,
        IRef<IDeclaration>? conflictingDeclaration )
    {
        this.Outcome = outcome;
        this.AdviceKind = adviceKind;
        this._declaration = declaration;
        this._conflictingDeclaration = conflictingDeclaration;
    }

    public IntroductionAdviceResult() { }

    [Memo]
    public T Declaration => this.Resolve( this._declaration );

    [Memo]
    public IDeclaration ConflictingDeclaration => this.Resolve( this._conflictingDeclaration );
}