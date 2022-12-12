// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Templating;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.Transformations
{
    /// <summary>
    /// Represents a method override, which redirects to another method without requiring template expansion.
    /// </summary>
    internal sealed class RedirectMethodTransformation : OverrideMemberTransformation
    {
        public new IMethod OverriddenDeclaration => (IMethod) base.OverriddenDeclaration;

        public IMethod TargetMethod { get; }

        public RedirectMethodTransformation( Advice advice, IMethod overriddenDeclaration, IMethod targetMethod, IObjectReader tags )
            : base( advice, overriddenDeclaration, tags )
        {
            Invariant.Assert( targetMethod != null );

            this.TargetMethod = targetMethod;
        }

        public override IEnumerable<InjectedMember> GetInjectedMembers( MemberInjectionContext context )
        {
            var body =
                SyntaxFactoryEx.FormattedBlock(
                    this.OverriddenDeclaration.ReturnType
                    != this.OverriddenDeclaration.Compilation.GetCompilationModel().Factory.GetTypeByReflectionType( typeof(void) )
                        ? ReturnStatement(
                            Token( SyntaxKind.ReturnKeyword ).WithTrailingTrivia( Space ),
                            GetInvocationExpression(),
                            Token( SyntaxKind.SemicolonToken ) )
                        : ExpressionStatement( GetInvocationExpression() ) );

            return new[]
            {
                new InjectedMember(
                    this,
                    MethodDeclaration(
                        List<AttributeListSyntax>(),
                        this.OverriddenDeclaration.GetSyntaxModifierList(),
                        context.SyntaxGenerator.ReturnType( this.OverriddenDeclaration ).WithTrailingTrivia( Space ),
                        null,
                        Identifier(
                            context.InjectionNameProvider.GetOverrideName(
                                this.OverriddenDeclaration.DeclaringType,
                                this.ParentAdvice.AspectLayerId,
                                this.OverriddenDeclaration ) ),
                        context.SyntaxGenerator.TypeParameterList( this.OverriddenDeclaration, context.Compilation ),
                        context.SyntaxGenerator.ParameterList( this.OverriddenDeclaration, context.Compilation ),
                        context.SyntaxGenerator.ConstraintClauses( this.OverriddenDeclaration ),
                        body,
                        null ),
                    this.ParentAdvice.AspectLayerId,
                    InjectedMemberSemantic.Override,
                    this.OverriddenDeclaration )
            };

            ExpressionSyntax GetInvocationExpression()
            {
                return
                    InvocationExpression(
                        GetInvocationTargetExpression(),
                        ArgumentList(
                            SeparatedList( this.OverriddenDeclaration.Parameters.SelectAsEnumerable( p => Argument( IdentifierName( p.Name ) ) ) ) ) );
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
                    .WithAspectReferenceAnnotation( this.ParentAdvice.AspectLayerId, AspectReferenceOrder.Base );
            }
        }
    }
}