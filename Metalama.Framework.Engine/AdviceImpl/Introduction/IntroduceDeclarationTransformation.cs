﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Introspection;
using System;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.AdviceImpl.Introduction;

internal abstract class IntroduceDeclarationTransformation<T> : BaseSyntaxTreeTransformation, IIntroduceDeclarationTransformation,
                                                                IInjectMemberTransformation
    where T : DeclarationBuilder
{
    public T IntroducedDeclaration { get; }

    protected IntroduceDeclarationTransformation( Advice advice, T introducedDeclaration ) : base( advice )
    {
        this.IntroducedDeclaration = introducedDeclaration.AssertNotNull();
    }

    public abstract IEnumerable<InjectedMember> GetInjectedMembers( MemberInjectionContext context );

    public virtual InsertPosition InsertPosition => this.IntroducedDeclaration.ToInsertPosition();

    IDeclarationBuilder IIntroduceDeclarationTransformation.DeclarationBuilder => this.IntroducedDeclaration;

    public override IDeclaration TargetDeclaration => this.IntroducedDeclaration.ContainingDeclaration.AssertNotNull();

    public override IntrospectionTransformationKind TransformationKind => IntrospectionTransformationKind.IntroduceMember;

    public override FormattableString ToDisplayString() => $"Introduce {this.IntroducedDeclaration.DeclarationKind} '{this.IntroducedDeclaration}'.";
}