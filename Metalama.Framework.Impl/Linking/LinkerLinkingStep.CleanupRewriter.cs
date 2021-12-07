// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Formatting;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Linking
{
    internal partial class LinkerLinkingStep
    {
        private class CleanupRewriter : CSharpSyntaxRewriter
        {
            // TODO: Prune what we visit.
            // TODO: Optimize (this reallocates multiple times.

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

                            // TODO: Solve trivia!
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

                        newStatements.Add( (StatementSyntax) rewritten.AssertNotNull() );
                    }
                }

                var finalStatements = new List<StatementSyntax>();

                // Process labeled statements.
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
                                    labeledStatement.Identifier.AddGeneratedCodeAnnotation(),
                                    Token( SyntaxKind.ColonToken ).AddGeneratedCodeAnnotation(),
                                    newStatements[i + 1] ) );

                            i++;
                        }
                    }
                    else
                    {
                        finalStatements.Add( statement );
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
                }
            }
        }
    }
}