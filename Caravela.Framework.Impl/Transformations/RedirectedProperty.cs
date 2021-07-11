// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.Advices;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Transformations
{
    /// <summary>
    /// Represents a property override, which redirects to accessors of another property without requiring template expansion.
    /// </summary>
    internal class RedirectedProperty : OverriddenMember
    {
        public new IProperty OverriddenDeclaration => (IProperty) base.OverriddenDeclaration;

        public IProperty TargetProperty { get; }

        public RedirectedProperty( Advice advice, IProperty overriddenDeclaration, IProperty targetProperty, AspectLinkerOptions? linkerOptions = null )
            : base( advice, overriddenDeclaration, linkerOptions )
        {
            Invariant.Assert( targetProperty != null );

            this.TargetProperty = targetProperty;
        }

        public override IEnumerable<IntroducedMember> GetIntroducedMembers( in MemberIntroductionContext context )
        {
            return new[]
            {
                new IntroducedMember(
                    this,
                    PropertyDeclaration(
                        List<AttributeListSyntax>(),
                        this.OverriddenDeclaration.GetSyntaxModifierList(),
                        this.OverriddenDeclaration.GetSyntaxReturnType(),
                        null,
                        Identifier( context.IntroductionNameProvider.GetOverrideName( this.Advice.AspectLayerId, this.OverriddenDeclaration ) ),
                        AccessorList( List( GetAccessors() ) ),
                        null,
                        null ),
                    this.Advice.AspectLayerId,
                    IntroducedMemberSemantic.Override,
                    this.OverriddenDeclaration )
            };

            IReadOnlyList<AccessorDeclarationSyntax> GetAccessors()
            {
                return new[]
                    {
                        this.OverriddenDeclaration.Getter != null
                            ? AccessorDeclaration(
                                SyntaxKind.GetAccessorDeclaration,
                                List<AttributeListSyntax>(),
                                this.OverriddenDeclaration.Getter.GetSyntaxModifierList(),
                                CreateGetterBody(),
                                null )
                            : null,
                        this.OverriddenDeclaration.Setter != null
                            ? AccessorDeclaration(
                                this.OverriddenDeclaration.Writeability != Writeability.InitOnly
                                    ? SyntaxKind.SetAccessorDeclaration
                                    : SyntaxKind.InitAccessorDeclaration,
                                List<AttributeListSyntax>(),
                                this.OverriddenDeclaration.Setter.GetSyntaxModifierList(),
                                CreateSetterBody(),
                                null )
                            : null
                    }.Where( a => a != null )
                    .AssertNoneNull()
                    .ToArray();
            }

            BlockSyntax CreateGetterBody()
            {
                return
                    Block( ReturnStatement( CreateAccessTargetExpression() ) );
            }

            BlockSyntax CreateSetterBody()
            {
                return
                    Block(
                        ExpressionStatement(
                            AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                CreateAccessTargetExpression(),
                                IdentifierName( "value" ) ) ) );
            }

            ExpressionSyntax CreateAccessTargetExpression()
            {
                return
                    this.OverriddenDeclaration.IsStatic
                        ? IdentifierName( this.OverriddenDeclaration.Name )
                        : MemberAccessExpression( SyntaxKind.SimpleMemberAccessExpression, ThisExpression(), IdentifierName( this.OverriddenDeclaration.Name ) )
                            .WithAspectReferenceAnnotation( new AspectReferenceSpecification( this.Advice.AspectLayerId, AspectReferenceOrder.Default ) );
            }
        }
    }
}