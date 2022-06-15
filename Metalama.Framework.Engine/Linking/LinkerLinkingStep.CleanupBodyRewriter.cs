// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Formatting;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.Linking
{
    internal partial class LinkerLinkingStep
    {
        private class CleanupBodyRewriter : CSharpSyntaxRewriter
        {
            // TODO: Optimize (this reallocates multiple times).

            public override SyntaxNode? VisitBlock( BlockSyntax node )
            {
                var anyRewrittenStatement = false;
                var newStatements = new List<StatementSyntax>();

                foreach ( var statement in node.Statements )
                {
                    if ( statement is BlockSyntax innerBlock )
                    {
                        var innerBlockFlags = innerBlock.GetLinkerGeneratedFlags();

                        if ( innerBlockFlags.HasFlag( LinkerGeneratedFlags.FlattenableBlock ) )
                        {
                            anyRewrittenStatement = true;

                            AddFlattenedBlockStatements( innerBlock, newStatements );
                        }
                        else
                        {
                            var rewritten = this.VisitBlock( innerBlock );

                            if ( rewritten != statement )
                            {
                                anyRewrittenStatement = true;
                            }

                            newStatements.Add( (StatementSyntax) rewritten.AssertNotNull() );
                        }
                    }
                    else
                    {
                        var rewritten = this.Visit( statement );

                        if ( rewritten != statement )
                        {
                            anyRewrittenStatement = true;
                        }

                        if ( rewritten != null )
                        {
                            newStatements.Add( (StatementSyntax) rewritten.AssertNotNull() );
                        }
                    }
                }

                var finalStatements = new List<StatementSyntax>();
                var overflowingTrivia = SyntaxTriviaList.Empty;

                // Process statements, cleaning empty labeled statements, and trivia empty statements and invocations with empty empty expressions.
                for ( var i = 0; i < newStatements.Count; i++ )
                {
                    var statement = newStatements[i];

                    if ( statement.GetLinkerGeneratedFlags().HasFlag( LinkerGeneratedFlags.EmptyLabeledStatement ) )
                    {
                        var labeledStatement = (statement as LabeledStatementSyntax).AssertNotNull();

                        if ( i == newStatements.Count - 1 )
                        {
                            finalStatements.Add( labeledStatement );
                        }
                        else
                        {
                            finalStatements.Add(
                                LabeledStatement(
                                    labeledStatement.Identifier.WithGeneratedCodeAnnotation( FormattingAnnotations.SystemGeneratedCodeAnnotation ),
                                    Token( SyntaxKind.ColonToken ).WithGeneratedCodeAnnotation( FormattingAnnotations.SystemGeneratedCodeAnnotation ),
                                    newStatements[i + 1] ) );

                            i++;
                        }

                        anyRewrittenStatement = true;
                    }
                    else if ( statement.GetLinkerGeneratedFlags().HasFlag( LinkerGeneratedFlags.EmptyTriviaStatement ) )
                    {
                        // This is statement that carries only trivias and should be removed, trivias added to the previous and next statement.
                        if ( finalStatements.Count == 0 )
                        {
                            // There is not yet any statement to attach the trivia so everything goes into overflow.
                            overflowingTrivia = statement.GetLeadingTrivia().AddRange( statement.GetTrailingTrivia() );
                        }
                        else
                        {
                            // We need to replace trailing trivia of the last statement.
                            var newTrailingTrivia =
                                overflowingTrivia.Count > 0
                                    ? statement.GetLeadingTrivia()
                                        .AddRange( finalStatements[finalStatements.Count - 1].GetTrailingTrivia() )
                                        .AddRange( statement.GetLeadingTrivia() )
                                    : finalStatements[finalStatements.Count - 1].GetTrailingTrivia().AddRange( statement.GetLeadingTrivia() );

                            finalStatements[finalStatements.Count - 1] =
                                finalStatements[finalStatements.Count - 1]
                                    .WithTrailingTrivia( newTrailingTrivia );

                            overflowingTrivia = statement.GetTrailingTrivia().StripFirstTrailingNewLine();
                        }

                        anyRewrittenStatement = true;
                    }
                    else
                    {
                        finalStatements.Add( statement );
                    }
                }

                if ( overflowingTrivia != null )
                {
                    if ( finalStatements.Count > 0 )
                    {
                        finalStatements[finalStatements.Count - 1] =
                            finalStatements[finalStatements.Count - 1]
                                .WithTrailingTrivia( finalStatements[finalStatements.Count - 1].GetTrailingTrivia().AddRange( overflowingTrivia ) );
                    }
                    else
                    {
                        node = node.WithCloseBraceToken(
                            node.CloseBraceToken.WithLeadingTrivia( overflowingTrivia.AddRange( node.CloseBraceToken.LeadingTrivia ) ) );
                    }
                }

                if ( anyRewrittenStatement )
                {
                    return node.Update( node.OpenBraceToken, List( finalStatements ), node.CloseBraceToken );
                }
                else
                {
                    return node;
                }

                void AddFlattenedBlockStatements( BlockSyntax block, List<StatementSyntax> statements )
                {
                    // Remember the predicted index of the first statement in the inlined block, which will receive trivia from open brace token.
                    var firstStatementIndex = statements.Count;

                    foreach ( var statement in block.Statements )
                    {
                        if ( statement is BlockSyntax innerBlock && innerBlock.GetLinkerGeneratedFlags().HasFlag( LinkerGeneratedFlags.FlattenableBlock ) )
                        {
                            AddFlattenedBlockStatements( innerBlock, statements );
                        }
                        else
                        {
                            var visitedStatement = (StatementSyntax?) this.Visit( statement );

                            if ( visitedStatement != null )
                            {
                                statements.Add( visitedStatement.WithFormattingAnnotationsFrom( block ) );
                            }
                        }
                    }

                    // Index of the last statement.
                    var lastStatementIndex = statements.Count - 1;
                    var leadingTrivia = block.OpenBraceToken.LeadingTrivia.AddRange( block.OpenBraceToken.TrailingTrivia.StripFirstTrailingNewLine() );
                    var trailingTrivia = block.CloseBraceToken.LeadingTrivia.AddRange( block.CloseBraceToken.TrailingTrivia.StripFirstTrailingNewLine() );

                    if ( firstStatementIndex >= statements.Count )
                    {
                        // There was no statement added.
                        // We will add an empty statement to carry trivia, which we will prune above.
                        statements.Add(
                            EmptyStatement()
                                .WithLeadingTrivia( leadingTrivia )
                                .WithTrailingTrivia( trailingTrivia )
                                .WithLinkerGeneratedFlags( LinkerGeneratedFlags.EmptyTriviaStatement ) );
                    }
                    else
                    {
                        statements[firstStatementIndex] =
                            statements[firstStatementIndex].WithLeadingTrivia( leadingTrivia.AddRange( statements[firstStatementIndex].GetLeadingTrivia() ) );

                        statements[lastStatementIndex] =
                            statements[lastStatementIndex].WithTrailingTrivia( statements[lastStatementIndex].GetTrailingTrivia().AddRange( trailingTrivia ) );
                    }
                }
            }

            public override SyntaxNode? VisitInvocationExpression( InvocationExpressionSyntax node )
            {
                if ( node.Expression.GetLinkerGeneratedFlags().HasFlag( LinkerGeneratedFlags.NullAspectReferenceExpression ) )
                {
                    return IdentifierName( "__LINKER_TO_BE_REMOVED__" )
                        .WithLinkerGeneratedFlags( LinkerGeneratedFlags.NullAspectReferenceExpression );
                }

                return node;
            }

            public override SyntaxNode? VisitExpressionStatement( ExpressionStatementSyntax node )
            {
                var transformed = (ExpressionSyntax) this.Visit( node.Expression ).AssertNotNull();

                if ( transformed.GetLinkerGeneratedFlags().HasFlag( LinkerGeneratedFlags.NullAspectReferenceExpression ) )
                {
                    return null;
                }

                if ( node.Expression != transformed )
                {
                    return node.WithExpression( transformed );
                }
                else
                {
                    return node;
                }
            }
        }
    }
}