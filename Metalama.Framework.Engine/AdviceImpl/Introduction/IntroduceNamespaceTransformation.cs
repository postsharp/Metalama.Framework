// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Introspection;
using System;

namespace Metalama.Framework.Engine.AdviceImpl.Introduction;

internal sealed class IntroduceNamespaceTransformation : BaseTransformation, IIntroduceDeclarationTransformation
{
    private NamespaceBuilder _introducedDeclaration;

    public IntroduceNamespaceTransformation( Advice advice, NamespaceBuilder introducedDeclaration ) : base( advice )
    {
        this._introducedDeclaration = introducedDeclaration.AssertNotNull();
    }

    public override TransformationObservability Observability => TransformationObservability.Always;

    IDeclarationBuilder IIntroduceDeclarationTransformation.DeclarationBuilder => this._introducedDeclaration;

    public override IDeclaration TargetDeclaration => this._introducedDeclaration.ContainingDeclaration.AssertNotNull();

    public override TransformationKind TransformationKind => TransformationKind.IntroduceMember;

    public override FormattableString ToDisplayString() => $"Introduce {this._introducedDeclaration.DeclarationKind} '{this._introducedDeclaration}'.";
}