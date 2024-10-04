// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Helpers;
using Metalama.Framework.Engine.CodeModel.Introductions.Builders;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Introspection;
using Microsoft.CodeAnalysis;
using System;

namespace Metalama.Framework.Engine.AdviceImpl.Attributes;

internal sealed class IntroduceAttributeTransformation : BaseSyntaxTreeTransformation, IIntroduceDeclarationTransformation
{
    public AttributeBuilder AttributeBuilder { get; }

    public IntroduceAttributeTransformation( Advice advice, AttributeBuilder attributeBuilder ) : base( advice )
    {
        this.AttributeBuilder = attributeBuilder;
    }

    public override IDeclaration TargetDeclaration => this.AttributeBuilder.ContainingDeclaration;

    public override TransformationObservability Observability => TransformationObservability.CompileTimeOnly;

    public override IntrospectionTransformationKind TransformationKind => IntrospectionTransformationKind.IntroduceAttribute;

    public IDeclarationBuilder DeclarationBuilder => this.AttributeBuilder;

    public override SyntaxTree TransformedSyntaxTree
        => this.DeclarationBuilder.ContainingDeclaration.AssertNotNull().GetPrimarySyntaxTree()
           ?? ((CompilationModel) this.DeclarationBuilder.Compilation).PartialCompilation.SyntaxTreeForCompilationLevelAttributes;

    public override FormattableString ToDisplayString() => $"Introduce attribute of type '{this.AttributeBuilder.Type}' into '{this.TargetDeclaration}'";
}