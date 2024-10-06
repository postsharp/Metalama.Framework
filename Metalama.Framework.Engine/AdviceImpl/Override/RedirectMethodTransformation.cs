// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Helpers;
using Metalama.Framework.Engine.SyntaxGeneration;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.AdviceImpl.Override;

/// <summary>
/// Represents a method override, which redirects to another method without requiring template expansion.
/// </summary>
internal sealed class RedirectMethodTransformation : OverrideMemberTransformation
{
    private readonly IRef<IMethod> _targetMethod;

    private new IRef<IMethod> OverriddenDeclaration => (IRef<IMethod>) base.OverriddenDeclaration;

    public RedirectMethodTransformation( Advice advice, IRef<IMethod> overriddenDeclaration, IRef<IMethod> targetMethod )
        : base( advice, overriddenDeclaration, ObjectReader.Empty )
    {
        this._targetMethod = targetMethod;
    }

    public override IEnumerable<InjectedMember> GetInjectedMembers( MemberInjectionContext context )
    {
        var overriddenDeclaration = this.OverriddenDeclaration.GetTarget(context.Compilation);

        var body =
            context.SyntaxGenerationContext.SyntaxGenerator.FormattedBlock(
                overriddenDeclaration.ReturnType
                != overriddenDeclaration.Compilation.GetCompilationModel().Cache.SystemVoidType
                    ? ReturnStatement(
                        SyntaxFactoryEx.TokenWithTrailingSpace( SyntaxKind.ReturnKeyword ),
                        GetInvocationExpression(),
                        Token( SyntaxKind.SemicolonToken ) )
                    : ExpressionStatement( GetInvocationExpression() ) );

        return
        [
            new InjectedMember(
                this,
                MethodDeclaration(
                    List<AttributeListSyntax>(),
                    overriddenDeclaration.GetSyntaxModifierList(),
                    context.SyntaxGenerator.ReturnType( overriddenDeclaration )
                        .WithOptionalTrailingTrivia( ElasticSpace, context.SyntaxGenerationContext.Options ),
                    null,
                    Identifier(
                        context.InjectionNameProvider.GetOverrideName(
                            overriddenDeclaration.DeclaringType,
                            this.AspectLayerId,
                            overriddenDeclaration ) ),
                    context.SyntaxGenerator.TypeParameterList( overriddenDeclaration, context.Compilation ),
                    context.SyntaxGenerator.ParameterList( overriddenDeclaration, context.Compilation, removeDefaultValues: true ),
                    context.SyntaxGenerator.ConstraintClauses( overriddenDeclaration ),
                    body,
                    null ),
                this.AspectLayerId,
                InjectedMemberSemantic.Override,
                overriddenDeclaration.ToRef() )
        ];

        ExpressionSyntax GetInvocationExpression()
        {
            return
                InvocationExpression(
                    GetInvocationTargetExpression(),
                    ArgumentList( SeparatedList( overriddenDeclaration.Parameters.SelectAsReadOnlyList( p => Argument( IdentifierName( p.Name ) ) ) ) ) );
        }

        ExpressionSyntax GetInvocationTargetExpression()
        {
            var expression =
                overriddenDeclaration.IsStatic
                    ? (ExpressionSyntax) IdentifierName( this._targetMethod.Name )
                    : MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        ThisExpression(),
                        IdentifierName( this._targetMethod.Name ) );

            return expression
                .WithAspectReferenceAnnotation( this.AspectLayerId, AspectReferenceOrder.Previous );
        }
    }
}