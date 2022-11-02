// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Builders;
using Microsoft.CodeAnalysis;

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

    public IDeclarationBuilder DeclarationBuilder => this.AttributeBuilder;

    public override SyntaxTree TransformedSyntaxTree
        => this.DeclarationBuilder.ContainingDeclaration.AssertNotNull().GetPrimarySyntaxTree()
           ?? ((CompilationModel) this.DeclarationBuilder.Compilation).PartialCompilation.SyntaxTreeForCompilationLevelAttributes;
}