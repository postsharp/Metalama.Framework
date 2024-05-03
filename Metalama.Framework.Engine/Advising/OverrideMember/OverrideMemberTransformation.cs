// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.SyntaxGeneration;
using Metalama.Framework.Introspection;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.Transformations;

internal abstract class OverrideMemberTransformation : BaseTransformation, IInjectMemberTransformation, IOverrideDeclarationTransformation
{
    protected IObjectReader Tags { get; }

    public IMember OverriddenDeclaration { get; }

    IDeclaration IOverrideDeclarationTransformation.OverriddenDeclaration => this.OverriddenDeclaration;

    public override IDeclaration TargetDeclaration => this.OverriddenDeclaration;

    protected OverrideMemberTransformation( Advice advice, IMember overriddenDeclaration, IObjectReader tags ) : base( advice )
    {
        Invariant.Assert( advice != null! );
        Invariant.Assert( overriddenDeclaration != null! );

        this.OverriddenDeclaration = overriddenDeclaration;
        this.Tags = tags;
    }

    public abstract IEnumerable<InjectedMember> GetInjectedMembers( MemberInjectionContext context );

    protected ExpressionSyntax CreateMemberAccessExpression( AspectReferenceTargetKind referenceTargetKind, SyntaxGenerationContext generationContext )
        => ProceedHelper.CreateMemberAccessExpression( this.OverriddenDeclaration, this.ParentAdvice.AspectLayerId, referenceTargetKind, generationContext );

    public InsertPosition InsertPosition => this.OverriddenDeclaration.ToInsertPosition();

    public override TransformationObservability Observability => TransformationObservability.None;

    public override FormattableString ToDisplayString() => $"Override the {this.OverriddenDeclaration.DeclarationKind} '{this.OverriddenDeclaration}'";

    public override TransformationKind TransformationKind => TransformationKind.OverrideMember;
}