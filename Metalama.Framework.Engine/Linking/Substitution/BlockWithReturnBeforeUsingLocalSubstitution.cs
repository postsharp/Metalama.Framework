// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.Linking.Substitution
{
    internal class BlockWithReturnBeforeUsingLocalSubstitution : SyntaxNodeSubstitution
    {
        public BlockSyntax RootBlock { get; }

        public BlockWithReturnBeforeUsingLocalSubstitution( BlockSyntax rootBlock )
        {
            this.RootBlock = rootBlock;
        }

        public override SyntaxNode TargetNode => this.RootBlock;

        public override SyntaxNode? Substitute( SyntaxNode currentNode, SubstitutionContext substitutionContext )
        {
            switch ( currentNode )
            {
                case BlockSyntax rootBlock:
                    var gotoStatementWalker = new GotoStatementWalker();
                    // PERF: Visits inlined bodies, leading to O(n^2) time complexity.
                    gotoStatementWalker.Visit( rootBlock );
                    var gotoStatements = gotoStatementWalker.GotoStatements;

                    var statementsContainingGoto = GetStatementsContainingGotoStatement( rootBlock, gotoStatements );

                    var encounteredStatementContainingGotoStatement = false;
                    var initialStatements = new List<StatementSyntax>();
                    LocalDeclarationStatementSyntax? usingLocalStatement = null;
                    var tailStatements = new List<StatementSyntax>();

                    foreach ( var statement in rootBlock.Statements )
                    {
                        if (statementsContainingGoto.Contains(statement))
                        {
                            encounteredStatementContainingGotoStatement = true;
                        }

                        if (statement is LocalDeclarationStatementSyntax localDeclaration && localDeclaration.UsingKeyword != null)
                        {
                            usingLocalStatement = localDeclaration;
                        }
                        else
                        {
                            if (usingLocalStatement == null)
                            {
                                initialStatements.Add(statement);                                
                            }
                            else
                            {
                                tailStatements.Add( statement );
                            }
                        }
                    }

                    if ( usingLocalStatement == null )
                    {
                        return currentNode;
                    }
                    else
                    {
                        initialStatements.Add( Translate( usingLocalStatement, tailStatements ) );

                        return rootBlock.WithStatements( List( initialStatements ) );
                    }

                default:
                    throw new AssertionFailedException();
            }

            static UsingStatementSyntax Translate( LocalDeclarationStatementSyntax local, List<StatementSyntax> statements )
            {
                return
                    UsingStatement(
                        Token( local.UsingKeyword.LeadingTrivia, SyntaxKind.UsingKeyword, local.UsingKeyword.TrailingTrivia ),
                        Token( TriviaList( ElasticMarker ), SyntaxKind.OpenParenToken, TriviaList( ElasticMarker ) ),
                        local.Declaration,
                        null,
                        Token( TriviaList( ElasticMarker ), SyntaxKind.CloseParenToken, TriviaList( ElasticMarker ) ),
                        Block(
                            Token( local.SemicolonToken.LeadingTrivia, SyntaxKind.OpenBraceToken, TriviaList( ElasticSpace ) ),
                            List( statements ),
                            Token( TriviaList( ElasticSpace ), SyntaxKind.CloseBraceToken, TriviaList() ) ) );
            }

            static HashSet<StatementSyntax> GetStatementsContainingGotoStatement( BlockSyntax rootBlock, IReadOnlyList<GotoStatementSyntax> gotoStatements )
            {
                var statementsContainingGotoStatement = new HashSet<StatementSyntax>();

                foreach ( var gotoStatement in gotoStatements )
                {
                    Mark( gotoStatement );

                    void Mark( SyntaxNode node )
                    {
                        if ( node is StatementSyntax statement )
                        {
                            if ( statementsContainingGotoStatement.Add( statement ) && statement != rootBlock )
                            {
                                // Process recursively unvisited statement that is not the root block.
                                Mark( statement.Parent );
                            }
                        }
                        else
                        {
                            // Process recursively the parent of a non-statement.
                            Mark( node.Parent );
                        }
                    }
                }

                return statementsContainingGotoStatement;
            }
        }

        private class GotoStatementWalker : CSharpSyntaxWalker
        {
            public List<GotoStatementSyntax> GotoStatements { get; }

            public GotoStatementWalker()
            {
                this.GotoStatements = new List<GotoStatementSyntax>();
            }

            public override void VisitGotoStatement( GotoStatementSyntax node )
            {
                this.GotoStatements.Add( node );
            }

            public override void Visit( SyntaxNode? node )
            {
                if ( node is not ExpressionSyntax and not LocalFunctionStatementSyntax)
                {
                    // Skip expressions and local functions.
                    base.Visit( node );
                }
            }
        }
    }
}