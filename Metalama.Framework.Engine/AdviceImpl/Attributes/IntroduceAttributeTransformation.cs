// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Introductions.BuilderData;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Introspection;
using System;

namespace Metalama.Framework.Engine.AdviceImpl.Attributes;

internal sealed class IntroduceAttributeTransformation : BaseSyntaxTreeTransformation, IIntroduceDeclarationTransformation
{
    public AttributeBuilderData BuilderData { get; }

    public IntroduceAttributeTransformation( AspectLayerInstance aspectLayerInstance, AttributeBuilderData builderData ) : base(
        aspectLayerInstance,
        builderData.ContainingDeclaration )
    {
        this.BuilderData = builderData;
    }

    public override IRef<IDeclaration> TargetDeclaration => this.BuilderData.ContainingDeclaration;

    public override TransformationObservability Observability => TransformationObservability.CompileTimeOnly;

    public override IntrospectionTransformationKind TransformationKind => IntrospectionTransformationKind.IntroduceAttribute;

    public DeclarationBuilderData DeclarationBuilderData => this.BuilderData;

    protected override FormattableString ToDisplayString( CompilationModel compilation )
        => $"Introduce attribute of type '{this.BuilderData.Type}' into '{this.TargetDeclaration}'";
}