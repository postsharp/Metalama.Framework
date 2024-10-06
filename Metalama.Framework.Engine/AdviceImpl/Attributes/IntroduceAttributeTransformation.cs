// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Introductions.Data;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Introspection;
using System;

namespace Metalama.Framework.Engine.AdviceImpl.Attributes;

internal sealed class IntroduceAttributeTransformation : BaseSyntaxTreeTransformation, IIntroduceDeclarationTransformation
{
    public AttributeBuilderData AttributeBuilder { get; }

    public IntroduceAttributeTransformation( Advice advice, AttributeBuilderData attributeBuilder ) : base( advice )
    {
        this.AttributeBuilder = attributeBuilder;
    }

    public override IRef<IDeclaration> TargetDeclaration => this.AttributeBuilder.ContainingDeclaration;

    public override TransformationObservability Observability => TransformationObservability.CompileTimeOnly;

    public override IntrospectionTransformationKind TransformationKind => IntrospectionTransformationKind.IntroduceAttribute;

    public DeclarationBuilderData DeclarationBuilderData => this.AttributeBuilder;

    public override FormattableString ToDisplayString( CompilationModel compilation )
        => $"Introduce attribute of type '{this.AttributeBuilder.Type}' into '{this.TargetDeclaration}'";
}