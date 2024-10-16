﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.Transformations;

namespace Metalama.Framework.Engine.AdviceImpl.Introduction;

internal abstract class IntroduceMemberTransformation<T> : IntroduceDeclarationTransformation<T>
    where T : MemberBuilder
{
    protected IntroduceMemberTransformation( Advice advice, T introducedDeclaration ) : base( advice, introducedDeclaration ) { }

    public override TransformationObservability Observability
        => this.IntroducedDeclaration.IsDesignTime ? TransformationObservability.Always : TransformationObservability.CompileTimeOnly;
}