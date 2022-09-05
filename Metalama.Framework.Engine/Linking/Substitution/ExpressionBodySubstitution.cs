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
    internal class ExpressionBodySubstitution : SyntaxNodeSubstitution
    {
        private readonly ArrowExpressionClauseSyntax _rootNode;
        private readonly IMethodSymbol _targetMethod;

        public ExpressionBodySubstitution( ArrowExpressionClauseSyntax rootNode, IMethodSymbol targetMethod )
        {
            this._rootNode = rootNode;
            this._targetMethod = targetMethod;
        }

        public override SyntaxNode TargetNode => this._rootNode;

        public override SyntaxNode? Substitute( SyntaxNode currentNode, SubstitutionContext substitutionContext )
        {
            switch ( currentNode )
            {
                case ArrowExpressionClauseSyntax arrowExpressionClause:
                    if ( this._targetMethod.ReturnsVoid )
                    {
                        return 
                            Block(
                                ExpressionStatement( arrowExpressionClause.Expression ) ) 
                            .WithLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock );
                    }
                    else
                    {
                        return
                            Block(
                                ReturnStatement(
                                    Token( arrowExpressionClause.Expression.GetLeadingTrivia(), SyntaxKind.ReturnKeyword, TriviaList( ElasticSpace ) ),
                                    arrowExpressionClause.Expression,
                                    Token( TriviaList(), SyntaxKind.SemicolonToken, arrowExpressionClause.Expression.GetTrailingTrivia().Add( ElasticLineFeed ) ) ) )
                            .WithLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock );
                    }
                default:
                    throw new AssertionFailedException();
            }
        }
    }
}