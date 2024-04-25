// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Advising;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Utilities;

namespace Metalama.Framework.Engine.Advising;

internal class IntroduceMemberAdviceResult<T> : AdviceResult, IIntroductionAdviceResult<T>
    where T : class, ICompilationElement
{
    private readonly IRef<IMember>? _conflictingMember;
    private readonly IRef<T>? _declaration;

    public IntroduceMemberAdviceResult(
        AdviceKind adviceKind,
        AdviceOutcome outcome,
        IRef<T>? declaration,
        IRef<IMember>? conflictingMember )
    {
        this.Outcome = outcome;
        this.AdviceKind = adviceKind;
        this._declaration = declaration;
        this._conflictingMember = conflictingMember;
    }

    public IntroduceMemberAdviceResult() { }

    [Memo]
    public T Declaration => this.Resolve( this._declaration );

    [Memo]
    public IMember ConflictingMember => this.Resolve( this._conflictingMember );
}