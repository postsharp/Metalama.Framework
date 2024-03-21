// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.SyntaxGeneration;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.Transformations;

/// <summary>
/// Represents a method override, which redirects to another method without requiring template expansion.
/// </summary>
internal sealed class RedirectMethodTransformation : OverrideMemberTransformation
{
    private readonly IMethod _targetMethod;

    private new IMethod OverriddenDeclaration => (IMethod) base.OverriddenDeclaration;

    public RedirectMethodTransformation( Advice advice, IMethod overriddenDeclaration, IMethod targetMethod )
        : base( advice, overriddenDeclaration, ObjectReader.Empty )
    {
        this._targetMethod = targetMethod;
    }

    public override IEnumerable<InjectedMember> GetInjectedMembers( MemberInjectionContext context )
    {
        var body =
            context.SyntaxGenerationContext.SyntaxGenerator.FormattedBlock(
                this.OverriddenDeclaration.ReturnType
                != this.OverriddenDeclaration.Compilation.GetCompilationModel().Cache.SystemVoidType
                    ? ReturnStatement(
                        SyntaxFactoryEx.TokenWithTrailingSpace( SyntaxKind.ReturnKeyword ),
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
                    context.SyntaxGenerator.ReturnType( this.OverriddenDeclaration )
                        .WithTrailingTriviaIfNecessary( ElasticSpace, context.SyntaxGenerationContext.Options ),
                    null,
                    Identifier(
                        context.InjectionNameProvider.GetOverrideName(
                            this.OverriddenDeclaration.DeclaringType,
                            this.ParentAdvice.AspectLayerId,
                            this.OverriddenDeclaration ) ),
                    context.SyntaxGenerator.TypeParameterList( this.OverriddenDeclaration, context.Compilation ),
                    context.SyntaxGenerator.ParameterList( this.OverriddenDeclaration, context.Compilation, true ),
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
                    ArgumentList( SeparatedList( this.OverriddenDeclaration.Parameters.SelectAsReadOnlyList( p => Argument( IdentifierName( p.Name ) ) ) ) ) );
        }

        ExpressionSyntax GetInvocationTargetExpression()
        {
            var expression =
                this.OverriddenDeclaration.IsStatic
                    ? (ExpressionSyntax) IdentifierName( this._targetMethod.Name )
                    : MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        ThisExpression(),
                        IdentifierName( this._targetMethod.Name ) );

            return expression
                .WithAspectReferenceAnnotation( this.ParentAdvice.AspectLayerId, AspectReferenceOrder.Previous );
        }
    }
}