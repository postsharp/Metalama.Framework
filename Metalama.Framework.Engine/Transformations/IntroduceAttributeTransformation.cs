// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel.Builders;

namespace Metalama.Framework.Engine.Transformations;

internal class IntroduceAttributeTransformation : BaseTransformation, IIntroduceDeclarationTransformation
{
    public AttributeBuilder AttributeBuilder { get; }

    public IntroduceAttributeTransformation( Advice advice, AttributeBuilder attributeBuilder ) : base( advice )
    {
        this.AttributeBuilder = attributeBuilder;
    }

    public override IDeclaration TargetDeclaration => this.AttributeBuilder.ContainingDeclaration;

    public override TransformationObservability Observability => TransformationObservability.CompileTimeOnly;

    public DeclarationBuilder DeclarationBuilder => this.AttributeBuilder;
}