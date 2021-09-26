// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.Advices;
using Caravela.Framework.Impl.Aspects;
using Caravela.Framework.Impl.Utilities;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Transformations
{
    /// <summary>
    /// Represents a method override, which redirects to another method without requiring template expansion.
    /// </summary>
    internal class RedirectedMethod : OverriddenMember
    {
        public new IMethod OverriddenDeclaration => (IMethod) base.OverriddenDeclaration;

        public IMethod TargetMethod { get; }

        public RedirectedMethod( Advice advice, IMethod overriddenDeclaration, IMethod targetMethod )
            : base( advice, overriddenDeclaration )
        {
            Invariant.Assert( targetMethod != null );

            this.TargetMethod = targetMethod;
        }

        public override IEnumerable<IntroducedMember> GetIntroducedMembers( in MemberIntroductionContext context )
        {
            var body =
                Block(
                    this.OverriddenDeclaration.ReturnType != this.OverriddenDeclaration.Compilation.TypeFactory.GetTypeByReflectionType( typeof(void) )
                        ? ReturnStatement( GetInvocationExpression() )
                        : ExpressionStatement( GetInvocationExpression() ) );

            return new[]
            {
                new IntroducedMember(
                    this,
                    MethodDeclaration(
                        List<AttributeListSyntax>(),
                        this.OverriddenDeclaration.GetSyntaxModifierList(),
                        context.SyntaxGenerator.ReturnType( this.OverriddenDeclaration ),
                        null,
                        Identifier(
                            context.IntroductionNameProvider.GetOverrideName(
                                this.OverriddenDeclaration.DeclaringType,
                                this.Advice.AspectLayerId,
                                this.OverriddenDeclaration ) ),
                        context.SyntaxGenerator.TypeParameterList( this.OverriddenDeclaration ),
                        context.SyntaxGenerator.ParameterList( this.OverriddenDeclaration ),
                        context.SyntaxGenerator.ConstraintClauses( this.OverriddenDeclaration ),
                        body,
                        null ),
                    this.Advice.AspectLayerId,
                    IntroducedMemberSemantic.Override,
                    this.OverriddenDeclaration )
            };

            ExpressionSyntax GetInvocationExpression()
            {
                return
                    InvocationExpression(
                        GetInvocationTargetExpression(),
                        ArgumentList( SeparatedList( this.OverriddenDeclaration.Parameters.Select( p => Argument( IdentifierName( p.Name ) ) ) ) ) );
            }

            ExpressionSyntax GetInvocationTargetExpression()
            {
                var expression =
                    this.OverriddenDeclaration.IsStatic
                        ? (ExpressionSyntax) IdentifierName( this.OverriddenDeclaration.Name )
                        : MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            ThisExpression(),
                            IdentifierName( this.OverriddenDeclaration.Name ) );

                return expression
                    .WithAspectReferenceAnnotation( this.Advice.AspectLayerId, AspectReferenceOrder.Base );
            }
        }
    }
}