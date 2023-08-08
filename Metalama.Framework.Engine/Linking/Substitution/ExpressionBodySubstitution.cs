// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Services;
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
        private readonly IMethodSymbol _referencingSymbol;
        private readonly IMethodSymbol _originalContainingSymbol;
        private readonly bool _usingSimpleInlining;
        private readonly string? _returnVariableIdentifier;

        public ExpressionBodySubstitution(
            CompilationContext compilationContext,
            ArrowExpressionClauseSyntax rootNode,
            IMethodSymbol referencingSymbol,
            IMethodSymbol originalContainingSymbol,
            bool usingSimpleInlining,
            string? returnVariableIdentifier = null ) : base( compilationContext )
        {
            Invariant.Implies( usingSimpleInlining, returnVariableIdentifier == null );
            Invariant.Implies( usingSimpleInlining, SymbolEqualityComparer.Default.Equals( referencingSymbol.ReturnType, originalContainingSymbol.ReturnType ) );
            Invariant.Implies( originalContainingSymbol.ReturnsVoid, this._returnVariableIdentifier == null );

            this._rootNode = rootNode;
            this._referencingSymbol = referencingSymbol;
            this._originalContainingSymbol = originalContainingSymbol;
            this._usingSimpleInlining = usingSimpleInlining;
            this._returnVariableIdentifier = returnVariableIdentifier;
        }

        public override SyntaxNode TargetNode => this._rootNode;

        public override SyntaxNode Substitute( SyntaxNode currentNode, SubstitutionContext substitutionContext )
        {
            switch ( currentNode )
            {
                case ArrowExpressionClauseSyntax { Expression: ThrowExpressionSyntax throwExpressionSyntax }:
                    {
                        return
                            SyntaxFactoryEx.FormattedBlock(
                                    ThrowStatement(
                                        throwExpressionSyntax.ThrowKeyword,
                                        throwExpressionSyntax.Expression,
                                        Token( SyntaxKind.SemicolonToken ) ) )
                                .WithLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock );
                    }

                case ArrowExpressionClauseSyntax arrowExpressionClause:
                    if ( this._usingSimpleInlining )
                    {
                        // Uses the simple inlining, i.e. generating simple return statement without any changes for non-void methods.
                        if ( this._referencingSymbol.ReturnsVoid )
                        {
                            return
                                SyntaxFactoryEx.FormattedBlock( ExpressionStatement( arrowExpressionClause.Expression ) )
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
                    else
                    {
                        if ( this._referencingSymbol.ReturnsVoid )
                        {
                            if ( this._originalContainingSymbol.ReturnsVoid )
                            {
                                // Both referencing and target methods return void, expression can be simply changed to 

                                return
                                    SyntaxFactoryEx.FormattedBlock( ExpressionStatement( arrowExpressionClause.Expression ) )
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
                                                    CastConditional( arrowExpressionClause.Expression ) ) ) )
                                        .WithLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock );
                            }
                        }
                        else
                        {
                            if ( this._returnVariableIdentifier != null )
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
                                if ( this._originalContainingSymbol.ReturnsVoid )
                                {
                                    Invariant.Assert( this._returnVariableIdentifier == null );

                                    return
                                        SyntaxFactoryEx.FormattedBlock( ExpressionStatement( arrowExpressionClause.Expression ) )
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
                                                        CastConditional( arrowExpressionClause.Expression ) ) ) )
                                            .WithLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock );
                                }
                            }
                        }
                    }

                default:
                    throw new AssertionFailedException( $"{currentNode.Kind()} is not supported." );
            }

            ExpressionSyntax CastConditional( ExpressionSyntax node )
            {
                // NOTE: See ReturnStatementSubstitution for details.

                if ( currentNode is not CastExpressionSyntax && !this._originalContainingSymbol.ReturnsVoid )
                {
                    return
                        CastExpression(
                            substitutionContext.SyntaxGenerationContext.SyntaxGenerator.Type( this._originalContainingSymbol.ReturnType ),
                            ParenthesizedExpression( node ) );
                }
                else
                {
                    return node;
                }
            }
        }
    }
}