// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.Linking
{
    internal sealed partial class LinkerLinkingStep
    {
        private sealed class RemoveTrailingReturnRewriter : SafeSyntaxRewriter
        {
            public override SyntaxNode VisitBlock( BlockSyntax node )
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

                    return node.PartialUpdate(
                        statements: List( newStatements ),
                        closeBraceToken: node.CloseBraceToken.WithLeadingTrivia(
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