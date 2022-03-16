// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.Linking
{
    internal partial class LinkerLinkingStep
    {
        private class RemoveTrailingReturnRewriter : CSharpSyntaxRewriter
        {
            public override SyntaxNode? VisitBlock( BlockSyntax node )
            {
                if ( node.Statements.Count > 0 && node.Statements.Last() is ReturnStatementSyntax { Expression: null } )
                {
                    var newStatements = new List<StatementSyntax>();

                    // Count return statements to be removed.
                    var returnStatementsToRemove = 0;

                    for ( var i = node.Statements.Count - 1; i >= 0; i-- )
                    {
                        if ( node.Statements[i] is ReturnStatementSyntax { Expression: null } )
                        {
                            returnStatementsToRemove++;
                        }
                        else
                        {
                            break;
                        }
                    }

                    // Copy everything before the return statement.
                    for ( var i = 0; i < node.Statements.Count - returnStatementsToRemove; i++ )
                    {
                        newStatements.Add( node.Statements[i] );
                    }

                    var additionalTrailingTrivia = TriviaList();

                    for ( var i = node.Statements.Count - returnStatementsToRemove; i < node.Statements.Count; i++ )
                    {
                        additionalTrailingTrivia =
                            additionalTrailingTrivia
                                .AddRange( node.Statements[i].GetLeadingTrivia() )
                                .AddRange( ((ReturnStatementSyntax) node.Statements[i]).SemicolonToken.TrailingTrivia.StripFirstTrailingNewLine() );
                    }

                    return node
                        .WithStatements( List( newStatements ) )
                        .WithCloseBraceToken(
                            node.CloseBraceToken.WithLeadingTrivia(
                                additionalTrailingTrivia
                                    .AddRange( node.CloseBraceToken.LeadingTrivia ) ) );
                }
                else
                {
                    return node;
                }
            }
        }
    }
}