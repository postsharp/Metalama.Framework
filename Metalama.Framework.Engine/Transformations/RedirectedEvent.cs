// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advices;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.Transformations
{
    /// <summary>
    /// Represents an event override, which redirects to accessors of another event without requiring template expansion.
    /// </summary>
    internal class RedirectedEvent : OverriddenMember
    {
        public new IEvent OverriddenDeclaration => (IEvent) base.OverriddenDeclaration;

        public IEvent TargetEvent { get; }

        public RedirectedEvent( Advice advice, IEvent overriddenDeclaration, IEvent targetEvent, IObjectReader tags )
            : base( advice, overriddenDeclaration, tags )
        {
            this.TargetEvent = targetEvent;
        }

        public override IEnumerable<IntroducedMember> GetIntroducedMembers( in MemberIntroductionContext context )
        {
            return new[]
            {
                new IntroducedMember(
                    this,
                    EventDeclaration(
                        List<AttributeListSyntax>(),
                        this.OverriddenDeclaration.GetSyntaxModifierList(),
                        context.SyntaxGenerator.EventType( this.OverriddenDeclaration ),
                        null,
                        Identifier(
                            context.IntroductionNameProvider.GetOverrideName(
                                this.OverriddenDeclaration.DeclaringType,
                                this.Advice.AspectLayerId,
                                this.OverriddenDeclaration ) ),
                        AccessorList( List( GetAccessors() ) ) ),
                    this.Advice.AspectLayerId,
                    IntroducedMemberSemantic.Override,
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
                    }.Where( a => a != null )
                    .ToArray();
            }

            BlockSyntax CreateAccessorBody( SyntaxKind assignmentKind )
            {
                return
                    Block(
                        ExpressionStatement(
                            AssignmentExpression(
                                assignmentKind,
                                CreateAccessTargetExpression(),
                                IdentifierName( "value" ) ) ) );
            }

            ExpressionSyntax CreateAccessTargetExpression()
            {
                return
                    this.OverriddenDeclaration.IsStatic
                        ? IdentifierName( this.OverriddenDeclaration.Name )
                        : MemberAccessExpression( SyntaxKind.SimpleMemberAccessExpression, ThisExpression(), IdentifierName( this.OverriddenDeclaration.Name ) )
                            .WithAspectReferenceAnnotation( this.Advice.AspectLayerId, AspectReferenceOrder.Base );
            }
        }
    }
}