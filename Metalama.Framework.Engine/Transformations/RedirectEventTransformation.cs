// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.Transformations;

/// <summary>
/// Represents an event override, which redirects to accessors of another event without requiring template expansion.
/// </summary>
internal sealed class RedirectEventTransformation : OverrideMemberTransformation
{
    private readonly IEvent _targetEvent;

    private new IEvent OverriddenDeclaration => (IEvent) base.OverriddenDeclaration;

    public RedirectEventTransformation( Advice advice, IEvent overriddenDeclaration, IEvent targetEvent )
        : base( advice, overriddenDeclaration, ObjectReader.Empty )
    {
        this._targetEvent = targetEvent;
    }

    public override IEnumerable<InjectedMember> GetInjectedMembers( MemberInjectionContext context )
    {
        return new[]
        {
            new InjectedMember(
                this,
                EventDeclaration(
                    List<AttributeListSyntax>(),
                    this.OverriddenDeclaration.GetSyntaxModifierList(),
                    context.SyntaxGenerator.EventType( this.OverriddenDeclaration ),
                    null,
                    Identifier(
                        context.InjectionNameProvider.GetOverrideName(
                            this.OverriddenDeclaration.DeclaringType,
                            this.ParentAdvice.AspectLayerId,
                            this.OverriddenDeclaration ) ),
                    AccessorList( List( GetAccessors() ) ) ),
                this.ParentAdvice.AspectLayerId,
                InjectedMemberSemantic.Override,
                this.OverriddenDeclaration )
        };

        IReadOnlyList<AccessorDeclarationSyntax> GetAccessors()
        {
            return new[]
                {
                    AccessorDeclaration(
                        SyntaxKind.AddAccessorDeclaration,
                        List<AttributeListSyntax>(),
                        this.OverriddenDeclaration.AddMethod.GetSyntaxModifierList(),
                        CreateAccessorBody( SyntaxKind.AddAssignmentExpression ),
                        null ),
                    AccessorDeclaration(
                        SyntaxKind.RemoveAccessorDeclaration,
                        List<AttributeListSyntax>(),
                        this.OverriddenDeclaration.RemoveMethod.GetSyntaxModifierList(),
                        CreateAccessorBody( SyntaxKind.SubtractAssignmentExpression ),
                        null )
                }.WhereNotNull()
                .ToArray();
        }

        BlockSyntax CreateAccessorBody( SyntaxKind assignmentKind )
        {
            return
                    context.SyntaxGenerationContext.SyntaxGenerator.FormattedBlock(
                    ExpressionStatement(
                        AssignmentExpression(
                            assignmentKind,
                            CreateAccessTargetExpression(),
                            IdentifierName( "value" ) ) ) );
        }

        ExpressionSyntax CreateAccessTargetExpression()
        {
            return
                this._targetEvent.IsStatic
                    ? IdentifierName( this._targetEvent.Name )
                    : MemberAccessExpression( SyntaxKind.SimpleMemberAccessExpression, ThisExpression(), IdentifierName( this._targetEvent.Name ) )
                        .WithAspectReferenceAnnotation( this.ParentAdvice.AspectLayerId, AspectReferenceOrder.Previous );
        }
    }
}