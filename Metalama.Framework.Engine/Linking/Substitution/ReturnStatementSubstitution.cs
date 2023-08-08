// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Compiler;
using Metalama.Framework.Engine.Formatting;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Templating;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.Linking.Substitution
{
    /// <summary>
    /// Substitutes the return statement based on current inlining context.
    /// </summary>
    internal sealed class ReturnStatementSubstitution : SyntaxNodeSubstitution
    {
        private readonly IMethodSymbol _containingSymbol;
        private readonly string? _returnVariableIdentifier;
        private readonly string? _returnLabelIdentifier;
        private readonly ITypeSymbol _returnType;
        private readonly bool _replaceByBreakIfOmitted;

        public override SyntaxNode TargetNode { get; }

        public ReturnStatementSubstitution(
            CompilationContext compilationContext,
            SyntaxNode returnNode,
            IMethodSymbol containingSymbol,
            string? returnVariableIdentifier,
            string? returnLabelIdentifier,
            ITypeSymbol returnType,
            bool replaceByBreakIfOmitted ) : base( compilationContext )
        {
            this.TargetNode = returnNode;
            this._containingSymbol = containingSymbol;
            this._returnVariableIdentifier = returnVariableIdentifier;
            this._returnLabelIdentifier = returnLabelIdentifier;
            this._returnType = returnType;
            this._replaceByBreakIfOmitted = replaceByBreakIfOmitted;
        }

        public override SyntaxNode Substitute( SyntaxNode currentNode, SubstitutionContext substitutionContext )
        {
            switch ( currentNode )
            {
                case ReturnStatementSyntax returnStatement:
                    if ( this._returnLabelIdentifier != null )
                    {
                        if ( returnStatement.Expression != null )
                        {
                            return
                                SyntaxFactoryEx.FormattedBlock(
                                        CreateAssignmentStatement( returnStatement.Expression )
                                            .WithTriviaFrom( returnStatement )
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
                            var assignmentStatement =
                                CreateAssignmentStatement( returnStatement.Expression )
                                    .WithTriviaFrom( returnStatement )
                                    .WithOriginalLocationAnnotationFrom( returnStatement );

                            if ( this._replaceByBreakIfOmitted )
                            {
                                return
                                    SyntaxFactoryEx.FormattedBlock(
                                            assignmentStatement,
                                            BreakStatement(
                                                Token( SyntaxKind.BreakKeyword ),
                                                Token( TriviaList(), SyntaxKind.SemicolonToken, TriviaList( ElasticLineFeed ) ) ) )
                                        .WithLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock );
                            }
                            else
                            {
                                return assignmentStatement;
                            }
                        }
                        else
                        {
                            if ( this._replaceByBreakIfOmitted )
                            {
                                return
                                    BreakStatement(
                                            Token( SyntaxKind.BreakKeyword ),
                                            Token( TriviaList(), SyntaxKind.SemicolonToken, TriviaList( ElasticLineFeed ) ) )
                                        .WithOriginalLocationAnnotationFrom( returnStatement );
                            }
                            else
                            {
                                return EmptyStatement()
                                    .WithOriginalLocationAnnotationFrom( returnStatement )
                                    .WithLinkerGeneratedFlags( LinkerGeneratedFlags.EmptyTriviaStatement );
                            }
                        }
                    }

                case ExpressionSyntax returnExpression:
                    if ( this._returnLabelIdentifier != null )
                    {
                        if ( this._containingSymbol.ReturnsVoid )
                        {
                            return
                                SyntaxFactoryEx.FormattedBlock(
                                        ExpressionStatement( returnExpression ).WithOriginalLocationAnnotationFrom( returnExpression ),
                                        CreateGotoStatement() )
                                    .WithLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock );
                        }
                        else
                        {
                            return
                                SyntaxFactoryEx.FormattedBlock(
                                        CreateAssignmentStatement( returnExpression ).WithOriginalLocationAnnotationFrom( returnExpression ),
                                        CreateGotoStatement() )
                                    .WithLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock );
                        }
                    }
                    else
                    {
                        var assignmentStatement =
                            CreateAssignmentStatement( returnExpression )
                                .WithOriginalLocationAnnotationFrom( returnExpression );

                        if ( this._replaceByBreakIfOmitted )
                        {
                            return
                                SyntaxFactoryEx.FormattedBlock(
                                        assignmentStatement,
                                        BreakStatement(
                                            Token( SyntaxKind.BreakKeyword ),
                                            Token( TriviaList(), SyntaxKind.SemicolonToken, TriviaList( ElasticLineFeed ) ) ) )
                                    .WithLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock );
                        }
                        else
                        {
                            return assignmentStatement;
                        }
                    }

                default:
                    throw new AssertionFailedException( $"{currentNode.Kind()} is not supported." );
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
                    identifier = SyntaxFactoryEx.DiscardIdentifier();

                    expression = SyntaxFactoryEx.SafeCastExpression(
                        this.CompilationContext.GetSyntaxGenerationContext( expression ).SyntaxGenerator.Type( this._returnType ),
                        expression );
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