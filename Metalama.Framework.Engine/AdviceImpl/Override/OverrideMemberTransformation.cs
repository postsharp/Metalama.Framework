// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Helpers;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Introspection;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.AdviceImpl.Override;

internal abstract class OverrideMemberTransformation : BaseSyntaxTreeTransformation, IInjectMemberTransformation, IOverrideDeclarationTransformation
{
    protected IObjectReader Tags { get; }

    public abstract IFullRef<IMember> OverriddenDeclaration { get; }

    IFullRef<IDeclaration> IOverrideDeclarationTransformation.OverriddenDeclaration => this.OverriddenDeclaration;

    public override IRef<IDeclaration> TargetDeclaration => this.OverriddenDeclaration;

    protected OverrideMemberTransformation( Advice advice, IObjectReader tags ) : base( advice )
    {
        Invariant.Assert( advice != null! );
        this.Tags = tags;
    }

    public abstract IEnumerable<InjectedMember> GetInjectedMembers( MemberInjectionContext context );

    protected ExpressionSyntax CreateMemberAccessExpression( AspectReferenceTargetKind referenceTargetKind, MemberInjectionContext context )
        => ProceedHelper.CreateMemberAccessExpression(
            this.OverriddenDeclaration.GetTarget( context.Compilation ),
            this.AspectLayerId,
            referenceTargetKind,
            context.SyntaxGenerationContext );

    public InsertPosition InsertPosition => this.OverriddenDeclaration.ToInsertPosition();

    public override TransformationObservability Observability => TransformationObservability.None;

    protected override FormattableString ToDisplayString( CompilationModel compilation )
        => $"Override the {this.OverriddenDeclaration.DeclarationKind} '{this.OverriddenDeclaration}'";

    public override IntrospectionTransformationKind TransformationKind => IntrospectionTransformationKind.OverrideMember;
}