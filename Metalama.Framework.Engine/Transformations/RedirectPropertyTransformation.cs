// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advices;
using Metalama.Framework.Engine.Advising;
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
    /// Represents a property override, which redirects to accessors of another property without requiring template expansion.
    /// </summary>
    internal class RedirectPropertyTransformation : OverrideMemberTransformation
    {
        public new IProperty OverriddenDeclaration => (IProperty) base.OverriddenDeclaration;

        public IProperty TargetProperty { get; }

        public RedirectPropertyTransformation( Advice advice, IProperty overriddenDeclaration, IProperty targetProperty, IObjectReader tags )
            : base( advice, overriddenDeclaration, tags )
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
                        context.SyntaxGenerator.PropertyType( this.OverriddenDeclaration ),
                        null,
                        Identifier(
                            context.IntroductionNameProvider.GetOverrideName(
                                this.OverriddenDeclaration.DeclaringType,
                                this.ParentAdvice.AspectLayerId,
                                this.OverriddenDeclaration ) ),
                        AccessorList( List( GetAccessors() ) ),
                        null,
                        null ),
                    this.ParentAdvice.AspectLayerId,
                    IntroducedMemberSemantic.Override,
                    this.OverriddenDeclaration )
            };

            IReadOnlyList<AccessorDeclarationSyntax> GetAccessors()
            {
                return new[]
                    {
                        this.OverriddenDeclaration.GetMethod != null
                            ? AccessorDeclaration(
                                SyntaxKind.GetAccessorDeclaration,
                                List<AttributeListSyntax>(),
                                this.OverriddenDeclaration.GetMethod.GetSyntaxModifierList(),
                                CreateGetterBody(),
                                null )
                            : null,
                        this.OverriddenDeclaration.SetMethod != null
                            ? AccessorDeclaration(
                                this.OverriddenDeclaration.Writeability != Writeability.InitOnly
                                    ? SyntaxKind.SetAccessorDeclaration
                                    : SyntaxKind.InitAccessorDeclaration,
                                List<AttributeListSyntax>(),
                                this.OverriddenDeclaration.SetMethod.GetSyntaxModifierList(),
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
                            .WithAspectReferenceAnnotation( this.ParentAdvice.AspectLayerId, AspectReferenceOrder.Base );
            }
        }
    }
}