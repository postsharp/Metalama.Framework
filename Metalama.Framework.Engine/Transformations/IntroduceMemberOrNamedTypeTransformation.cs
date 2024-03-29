// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Introspection;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.Transformations;

internal abstract class IntroduceMemberOrNamedTypeTransformation<T> : BaseTransformation, IIntroduceDeclarationTransformation,
                                                                      IInjectMemberOrNamedTypeTransformation
    where T : MemberOrNamedTypeBuilder
{
    protected IntroduceMemberOrNamedTypeTransformation( Advice advice, T introducedDeclaration ) : base( advice )
    {
        this.IntroducedDeclaration = introducedDeclaration.AssertNotNull();
    }

    public abstract IEnumerable<InjectedMemberOrNamedType> GetInjectedMembers( MemberInjectionContext context );

    public virtual InsertPosition InsertPosition => this.IntroducedDeclaration.ToInsertPosition();

    public T IntroducedDeclaration { get; }

    IDeclarationBuilder IIntroduceDeclarationTransformation.DeclarationBuilder => this.IntroducedDeclaration;

    public override IDeclaration TargetDeclaration => this.IntroducedDeclaration.ContainingDeclaration.AssertNotNull();

    public override SyntaxTree TransformedSyntaxTree => this.IntroducedDeclaration.PrimarySyntaxTree.AssertNotNull();

    public override TransformationKind TransformationKind => TransformationKind.IntroduceMember;

    public override FormattableString ToDisplayString() => $"Introduce {this.IntroducedDeclaration.DeclarationKind} '{this.IntroducedDeclaration}'.";
}