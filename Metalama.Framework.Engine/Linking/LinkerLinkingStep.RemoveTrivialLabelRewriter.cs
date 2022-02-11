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
        // this rewriter is temporary until we properly use results of Control Flow Analysis while inlining.
        private class RemoveTrivialLabelRewriter : CSharpSyntaxRewriter
        {
            private readonly IReadOnlyDictionary<string, int> _observedLabelCounter;

            public RemoveTrivialLabelRewriter(IReadOnlyDictionary<string, int> observedLabelCounter)
            {
                this._observedLabelCounter = observedLabelCounter;
            }

            public override SyntaxNode? VisitBlock( BlockSyntax node )
            {
                var newStatements = new List<StatementSyntax>();
                var overflowingTrivia = SyntaxTriviaList.Empty;
                var anyChange = false;

                for (var i = 0; i < node.Statements.Count - 1; i++ )
                {
                    var statement = node.Statements[i];
                    var nextStatement = node.Statements[i + 1];

                    if (statement is GotoStatementSyntax { Expression: IdentifierNameSyntax { Identifier: {ValueText: var gotoLabel } } }
                        && nextStatement is LabeledStatementSyntax { Identifier: { ValueText: var declaredLabel } }
                        && gotoLabel == declaredLabel )
                    {
                        if (this._observedLabelCounter.TryGetValue( declaredLabel, out var counter)
                            && counter == 1)
                        {
                            overflowingTrivia.AddRange( statement.GetLeadingTrivia() );
                            overflowingTrivia.AddRange( statement.GetTrailingTrivia() );
                            overflowingTrivia.AddRange( nextStatement.GetLeadingTrivia() );
                            overflowingTrivia.AddRange( nextStatement.GetTrailingTrivia() );

                            anyChange = true;

                            i++;
                            continue;
                        }
                    }

                    if ( overflowingTrivia.Count > 0 )
                    {
                        newStatements.Add(
                            statement.WithLeadingTrivia(
                                overflowingTrivia.AddRange( statement.GetLeadingTrivia() ) ) );

                        overflowingTrivia = SyntaxTriviaList.Empty;
                    }
                    else
                    {
                        newStatements.Add( statement );
                    }
                }

                if ( anyChange )
                {
                    return node.WithStatements( List( newStatements ) )
                        .WithCloseBraceToken(
                            node.CloseBraceToken.WithLeadingTrivia(
                                overflowingTrivia.AddRange( node.CloseBraceToken.LeadingTrivia ) ) );
                }
                else
                {
                    return node;
                }
            }
        }
    }
}