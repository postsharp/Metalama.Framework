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
                var anyChange = false;

                for (var i = 0; i < node.Statements.Count; i++ )
                {
                    var statement = node.Statements[i];
                    var nextStatement = i + 1 < node.Statements.Count ? node.Statements[i + 1] : null;

                    if (statement is GotoStatementSyntax { Expression: IdentifierNameSyntax { Identifier: {ValueText: var gotoLabel } } } gotoStatement
                        && nextStatement is LabeledStatementSyntax { Identifier: { ValueText: var declaredLabel } } labeledStatement
                        && gotoLabel == declaredLabel )
                    {
                        if ( this._observedLabelCounter.TryGetValue( declaredLabel, out var counter) && counter == 1 )
                        {
                            newStatements.Add(
                                labeledStatement.Statement
                                    .WithLeadingTrivia(
                                        TriviaList( ElasticMarker )
                                        .AddRange( gotoStatement.GetLeadingTrivia() )
                                        .AddRange( gotoStatement.GetTrailingTrivia().StripFirstTrailingNewLine() )
                                        .AddRange( TriviaList( ElasticMarker ) )
                                        .AddRange( labeledStatement.GetLeadingTrivia() )
                                        .AddRange( labeledStatement.Statement.GetLeadingTrivia() ) ) );                            

                            anyChange = true;

                            i++;
                            continue;
                        }
                    }

                    newStatements.Add( statement );
                }

                if ( anyChange )
                {
                    return node.WithStatements( List( newStatements ) );
                }
                else
                {
                    return node;
                }
            }
        }
    }
}