// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Advising;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Utilities;
using System;

namespace Metalama.Framework.Engine.Advising;

internal class AddAttributeAdviceResult : AdviceResult, IIntroductionAdviceResult<IAttribute>
{
    private readonly IRef<IAttribute>? _attribute;

    public AddAttributeAdviceResult()
    {
        this.AdviceKind = AdviceKind.IntroduceAttribute;
    }

    public AddAttributeAdviceResult( AdviceOutcome outcome, IRef<IAttribute> attribute )
    {
        this.AdviceKind = AdviceKind.IntroduceAttribute;
        this.Outcome = outcome;
        this._attribute = attribute;
    }

    [Memo]
    public IAttribute Declaration => this.Resolve( this._attribute );

    public IMember ConflictingMember => throw new NotSupportedException();
}