// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Introductions.Builders;
using Metalama.Framework.Engine.CodeModel.Introductions.Data;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Introspection;
using System;

namespace Metalama.Framework.Engine.AdviceImpl.Introduction;

internal sealed class IntroduceNamespaceTransformation : BaseTransformation, IIntroduceDeclarationTransformation
{
    private readonly NamespaceBuilderData _introducedDeclaration;

    public IntroduceNamespaceTransformation( Advice advice, NamespaceBuilderData introducedDeclaration ) : base( advice )
    {
        this._introducedDeclaration = introducedDeclaration.AssertNotNull();
    }

    public override TransformationObservability Observability => TransformationObservability.Always;

    DeclarationBuilderData IIntroduceDeclarationTransformation.DeclarationBuilderData => this._introducedDeclaration;

    public override IRef<IDeclaration> TargetDeclaration => this._introducedDeclaration.ContainingDeclaration.AssertNotNull();

    public override IntrospectionTransformationKind TransformationKind => IntrospectionTransformationKind.IntroduceMember;

    public override FormattableString ToDisplayString( CompilationModel compilation ) => $"Introduce {this._introducedDeclaration.DeclarationKind} '{this._introducedDeclaration}'.";
}