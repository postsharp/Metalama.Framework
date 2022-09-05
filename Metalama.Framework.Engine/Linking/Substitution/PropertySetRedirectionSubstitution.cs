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
    internal class PropertySetRedirectionSubstitution : SyntaxNodeSubstitution
    {
        private readonly SyntaxNode _rootNode;
        private readonly IPropertySymbol _targetProperty;

        public PropertySetRedirectionSubstitution( SyntaxNode rootNode, IPropertySymbol targetProperty )
        {
            this._rootNode = rootNode;
            this._targetProperty = targetProperty;
        }

        public override SyntaxNode TargetNode => this._rootNode;

        public override SyntaxNode? Substitute( SyntaxNode currentNode, SubstitutionContext substitutionContext )
        {
            return
                Block(
                    ExpressionStatement( AssignmentExpression( SyntaxKind.SimpleAssignmentExpression, CreateAccessExpression(), IdentifierName( "value" ) ) ) )
                .WithGeneratedCodeAnnotation( FormattingAnnotations.SystemGeneratedCodeAnnotation );

            InvocationExpressionSyntax CreateAccessExpression()
            {
                return InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        this._targetProperty.IsStatic
                        ? substitutionContext.SyntaxGenerationContext.SyntaxGenerator.Type( this._targetProperty.ContainingType )
                        : ThisExpression(),
                        IdentifierName( this._targetProperty.Name ) ) );
            }
        }
    }
}