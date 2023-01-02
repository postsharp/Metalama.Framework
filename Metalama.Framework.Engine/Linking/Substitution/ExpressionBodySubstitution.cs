﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Templating;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.Linking.Substitution
{
    internal sealed class ExpressionBodySubstitution : SyntaxNodeSubstitution
    {
        private readonly ArrowExpressionClauseSyntax _rootNode;
        private readonly IMethodSymbol _referencingMethod;
        private readonly IMethodSymbol _targetMethod;
        private readonly string? _returnVariableIdentifier;

        public ExpressionBodySubstitution( ArrowExpressionClauseSyntax rootNode, IMethodSymbol referencingMethod, IMethodSymbol targetMethod, string? returnVariableIdentifier = null )
        {
            this._rootNode = rootNode;
            this._referencingMethod = referencingMethod;
            this._targetMethod = targetMethod;
            this._returnVariableIdentifier = returnVariableIdentifier;
        }

        public override SyntaxNode TargetNode => this._rootNode;

        public override SyntaxNode Substitute( SyntaxNode currentNode, SubstitutionContext substitutionContext )
        {
            switch ( currentNode )
            {
                case ArrowExpressionClauseSyntax arrowExpressionClause:
                    if ( this._referencingMethod.ReturnsVoid )
                    {
                        if ( this._targetMethod.ReturnsVoid )
                        {
                            return
                                SyntaxFactoryEx.FormattedBlock(
                                    ExpressionStatement(
                                        arrowExpressionClause.Expression ) )
                                    .WithLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock );
                        }
                        else
                        {
                            return
                                SyntaxFactoryEx.FormattedBlock(
                                    ExpressionStatement(
                                        AssignmentExpression(
                                            SyntaxKind.SimpleAssignmentExpression,
                                            IdentifierName(
                                                Identifier(
                                                    TriviaList(),
                                                    SyntaxKind.UnderscoreToken,
                                                    "_",
                                                    "_",
                                                    TriviaList() ) ),
                                            arrowExpressionClause.Expression ) ) )
                                    .WithLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock );
                        }
                    }
                    else
                    {
                        if ( this._targetMethod.ReturnsVoid )
                        {
                            Invariant.Assert( this._returnVariableIdentifier == null );

                            return
                                SyntaxFactoryEx.FormattedBlock(
                                    ExpressionStatement(
                                        arrowExpressionClause.Expression ) )
                                    .WithLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock );
                        }
                        else if ( this._returnVariableIdentifier != null )
                        {
                            return
                                SyntaxFactoryEx.FormattedBlock(
                                        ExpressionStatement(
                                            AssignmentExpression(
                                                SyntaxKind.SimpleAssignmentExpression,
                                                IdentifierName( this._returnVariableIdentifier ),
                                                Token( TriviaList( ElasticSpace ), SyntaxKind.EqualsToken, TriviaList( ElasticSpace ) ),
                                                arrowExpressionClause.Expression ),
                                            Token( TriviaList(), SyntaxKind.SemicolonToken, TriviaList( ElasticLineFeed ) ) ) )
                                    .WithLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock );
                        }
                        else
                        {
                            return
                                SyntaxFactoryEx.FormattedBlock(
                                        ReturnStatement(
                                            Token( arrowExpressionClause.Expression.GetLeadingTrivia(), SyntaxKind.ReturnKeyword, TriviaList( Space ) ),
                                            arrowExpressionClause.Expression,
                                            Token(
                                                TriviaList(),
                                                SyntaxKind.SemicolonToken,
                                                arrowExpressionClause.Expression.GetTrailingTrivia().Add( ElasticLineFeed ) ) ) )
                                    .WithLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock );
                        }
                    }

                default:
                    throw new AssertionFailedException( $"{currentNode.Kind()} is not supported." );
            }
        }
    }
}