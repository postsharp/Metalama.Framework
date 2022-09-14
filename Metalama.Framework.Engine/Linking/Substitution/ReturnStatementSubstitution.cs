﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Compiler;
using Metalama.Framework.Engine.Formatting;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.Linking.Substitution
{
    /// <summary>
    /// Substitutes the return statement based on current inlining context.
    /// </summary>
    internal class ReturnStatementSubstitution : SyntaxNodeSubstitution
    {
        private readonly SyntaxNode _returnNode;
        private readonly IMethodSymbol _containingSymbol;
        private readonly string? _returnVariableIdentifier;
        private readonly string? _returnLabelIdentifier;

        public override SyntaxNode TargetNode => this._returnNode;

        public ReturnStatementSubstitution( SyntaxNode returnNode, IMethodSymbol containingSymbol, string? returnVariableIdentifier, string? returnLabelIdentifier )
        {
            this._returnNode = returnNode;
            this._containingSymbol = containingSymbol;
            this._returnVariableIdentifier = returnVariableIdentifier;
            this._returnLabelIdentifier = returnLabelIdentifier;
        }

        public override SyntaxNode? Substitute( SyntaxNode currentNode, SubstitutionContext substitutionContext )
        {
            if ( currentNode is ReturnStatementSyntax returnStatement )
            {
                if ( this._returnLabelIdentifier != null )
                {
                    if ( returnStatement.Expression != null )
                    {
                        return
                            Block(
                                CreateAssignmentStatement( returnStatement.Expression )
                                .WithLeadingTrivia( returnStatement.GetLeadingTrivia() )
                                .WithTrailingTrivia( returnStatement.GetTrailingTrivia() )
                                .WithOriginalLocationAnnotationFrom( returnStatement ),
                                CreateGotoStatement() )
                            .WithLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock );
                    }
                    else
                    {
                        return CreateGotoStatement();
                    }
                }
                else
                {
                    if ( returnStatement.Expression != null )
                    {
                        return 
                            CreateAssignmentStatement( returnStatement.Expression )
                            .WithLeadingTrivia( returnStatement.GetLeadingTrivia() )
                            .WithTrailingTrivia( returnStatement.GetTrailingTrivia() )
                            .WithOriginalLocationAnnotationFrom( returnStatement );
                    }
                    else
                    {
                        return EmptyStatement()
                            .WithOriginalLocationAnnotationFrom( returnStatement )
                            .WithLinkerGeneratedFlags(LinkerGeneratedFlags.EmptyTriviaStatement);
                    }
                }
            }
            else if ( currentNode is ExpressionSyntax returnExpression )
            {
                if ( this._returnLabelIdentifier != null )
                {
                    if ( this._containingSymbol.ReturnsVoid )
                    {
                        return
                            Block(
                                    ExpressionStatement( returnExpression ).WithOriginalLocationAnnotationFrom( returnExpression ),
                                    CreateGotoStatement() )
                                .WithLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock );
                    }
                    else
                    {
                        return
                            Block(
                                    CreateAssignmentStatement( returnExpression ).WithOriginalLocationAnnotationFrom( returnExpression ),
                                    CreateGotoStatement() )
                                .WithLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock );
                    }
                }
                else
                {
                    if ( this._containingSymbol.ReturnsVoid )
                    {
                        return 
                            ExpressionStatement( returnExpression )
                            .WithOriginalLocationAnnotationFrom( returnExpression );
                    }
                    else
                    {
                        return CreateAssignmentStatement( returnExpression ).WithOriginalLocationAnnotationFrom( returnExpression );
                    }
                }
            }
            else
            {
                throw new AssertionFailedException();
            }

            StatementSyntax CreateAssignmentStatement( ExpressionSyntax expression )
            {
                IdentifierNameSyntax identifier;

                if ( this._returnVariableIdentifier != null )
                {
                    identifier = IdentifierName( this._returnVariableIdentifier );
                }
                else
                {
                    identifier =
                        IdentifierName(
                            Identifier(
                                TriviaList(),
                                SyntaxKind.UnderscoreToken,
                                "_",
                                "_",
                                TriviaList() ) );
                }

                return
                    ExpressionStatement(
                            AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                identifier,
                                Token( TriviaList( ElasticSpace ), SyntaxKind.EqualsToken, TriviaList( ElasticSpace ) ),
                                expression ),
                            Token( SyntaxKind.SemicolonToken ).WithTrailingTrivia( ElasticLineFeed ) )
                        .WithGeneratedCodeAnnotation( FormattingAnnotations.SystemGeneratedCodeAnnotation );
            }

            GotoStatementSyntax CreateGotoStatement()
            {
                return
                    GotoStatement(
                        SyntaxKind.GotoStatement,
                        Token( SyntaxKind.GotoKeyword ).WithTrailingTrivia( ElasticSpace ),
                        default,
                        IdentifierName( this._returnLabelIdentifier.AssertNotNull() ),
                        Token( SyntaxKind.SemicolonToken ).WithTrailingTrivia( ElasticLineFeed ) )
                    .WithGeneratedCodeAnnotation( FormattingAnnotations.SystemGeneratedCodeAnnotation );
            }
        }
    }
}