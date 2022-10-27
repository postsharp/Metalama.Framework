// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Builders;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.Transformations;

internal abstract class IntroduceMemberTransformation<T> : BaseTransformation, IIntroduceDeclarationTransformation, IInjectMemberTransformation
    where T : MemberBuilder
{
    protected IntroduceMemberTransformation( Advice advice, T introducedDeclaration ) : base( advice )
    {
        this.IntroducedDeclaration = introducedDeclaration.AssertNotNull();
    }

    public abstract IEnumerable<InjectedMember> GetInjectedMembers( MemberInjectionContext context );

    public virtual InsertPosition InsertPosition => this.IntroducedDeclaration.ToInsertPosition();

    public T IntroducedDeclaration { get; }

    DeclarationBuilder IIntroduceDeclarationTransformation.DeclarationBuilder => this.IntroducedDeclaration;

    public override IDeclaration TargetDeclaration => this.IntroducedDeclaration.ContainingDeclaration.AssertNotNull();

    public override TransformationObservability Observability
        => this.IntroducedDeclaration.IsDesignTime ? TransformationObservability.Always : TransformationObservability.CompileTimeOnly;

    public override SyntaxTree TransformedSyntaxTree => this.IntroducedDeclaration.PrimarySyntaxTree.AssertNotNull();
}