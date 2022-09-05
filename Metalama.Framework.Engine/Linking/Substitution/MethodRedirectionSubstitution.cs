// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Formatting;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.Linking.Substitution
{
    internal class MethodRedirectionSubstitution : SyntaxNodeSubstitution
    {
        private readonly SyntaxNode _rootNode;
        private readonly IMethodSymbol _targetMethod;

        public MethodRedirectionSubstitution(SyntaxNode rootNode, IMethodSymbol targetMethod )
        {
            this._rootNode = rootNode;
            this._targetMethod = targetMethod;
        }

        public override SyntaxNode TargetNode => this._rootNode;

        public override SyntaxNode? Substitute( SyntaxNode currentNode, SubstitutionContext substitutionContext )
        {
            return
                Block(
                    this._targetMethod.ReturnsVoid
                    ? ExpressionStatement( CreateInvocationExpression() )
                    : ReturnStatement( CreateInvocationExpression() ) )
                .WithGeneratedCodeAnnotation( FormattingAnnotations.SystemGeneratedCodeAnnotation );

            InvocationExpressionSyntax CreateInvocationExpression()
            {
                return InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        this._targetMethod.IsStatic
                        ? substitutionContext.SyntaxGenerationContext.SyntaxGenerator.Type( this._targetMethod.ContainingType )
                        : ThisExpression(),
                        this._targetMethod.IsGenericMethod
                        ? GenericName(
                            Identifier( this._targetMethod.Name ),
                            TypeArgumentList( SeparatedList( this._targetMethod.TypeParameters.Select( tp => (TypeSyntax) IdentifierName( tp.Name ) ) ) ) )
                        : IdentifierName( this._targetMethod.Name ) ) );
            }
        }
    }
}