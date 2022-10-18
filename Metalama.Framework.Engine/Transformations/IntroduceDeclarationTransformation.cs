// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel.Builders;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.Transformations;

internal class IntroduceDeclarationTransformation<T> : BaseTransformation, IIntroduceDeclarationTransformation
    where T : DeclarationBuilder
{
    public IntroduceDeclarationTransformation( Advice advice, T introducedDeclaration ) : base( advice )
    {
        this.IntroducedDeclaration = introducedDeclaration;
    }

    public T IntroducedDeclaration { get; }

    DeclarationBuilder IIntroduceDeclarationTransformation.DeclarationBuilder => this.IntroducedDeclaration;

    public override IDeclaration TargetDeclaration => this.IntroducedDeclaration.ContainingDeclaration.AssertNotNull();

    public override TransformationObservability Observability
        => this.IntroducedDeclaration.IsDesignTime ? TransformationObservability.Always : TransformationObservability.CompileTimeOnly;

    public override SyntaxTree TransformedSyntaxTree => this.IntroducedDeclaration.PrimarySyntaxTree.AssertNotNull();
}