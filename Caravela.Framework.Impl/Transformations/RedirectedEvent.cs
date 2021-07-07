// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.Advices;
using Caravela.Framework.Impl.Linking;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Transformations
{
    /// <summary>
    /// Represents an event override, which redirects to accessors of another event without requiring template expansion.
    /// </summary>
    internal class RedirectedEvent : OverriddenMember
    {
        public new IEvent OverriddenDeclaration => (IEvent) base.OverriddenDeclaration;

        public IEvent TargetEvent { get; }

        public RedirectedEvent( Advice advice, IEvent overriddenDeclaration, IEvent targetEvent, AspectLinkerOptions? linkerOptions = null )
            : base( advice, overriddenDeclaration, linkerOptions )
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
                        this.OverriddenDeclaration.GetSyntaxReturnType(),
                        null,
                        Identifier( context.IntroductionNameProvider.GetOverrideName( this.Advice.AspectLayerId, this.OverriddenDeclaration ) ),
                        AccessorList( List( GetAccessors() ) ) ),
                    this.Advice.AspectLayerId,
                    IntroducedMemberSemantic.Override,
                    this.LinkerOptions,
                    this.OverriddenDeclaration )
            };

            IReadOnlyList<AccessorDeclarationSyntax> GetAccessors()
            {
                return new[]
                    {
                        AccessorDeclaration(
                            SyntaxKind.AddAccessorDeclaration,
                            List<AttributeListSyntax>(),
                            this.OverriddenDeclaration.Adder.GetSyntaxModifierList(),
                            CreateAccessorBody( SyntaxKind.AddAssignmentExpression ),
                            null ),
                        AccessorDeclaration(
                            SyntaxKind.RemoveAccessorDeclaration,
                            List<AttributeListSyntax>(),
                            this.OverriddenDeclaration.Remover.GetSyntaxModifierList(),
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
                            .AddLinkerAnnotation( new LinkerAnnotation( this.Advice.AspectLayerId, LinkingOrder.Default ) );
            }
        }
    }
}