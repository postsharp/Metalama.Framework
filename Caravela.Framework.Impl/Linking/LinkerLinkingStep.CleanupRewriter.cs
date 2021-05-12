// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

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

            public override SyntaxNode? VisitBlock( BlockSyntax node )
            {
                var anyRewrittenStatement = false;
                var newStatements = new List<StatementSyntax>();

                foreach ( var statement in node.Statements )
                {
                    if ( statement is BlockSyntax innerBlock )
                    {
                        var innerBlockFlags = innerBlock.GetLinkerGeneratedFlags();

                        if ( innerBlockFlags.HasFlag( LinkerGeneratedFlags.Flattenable ) )
                        {
                            anyRewrittenStatement = true;

                            foreach ( var innerBlockStatement in ((BlockSyntax) this.VisitBlock( innerBlock ).AssertNotNull()).Statements )
                            {
                                newStatements.Add( innerBlockStatement );
                            }

                            // TODO: Solve trivias!
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

                if ( anyRewrittenStatement )
                {
                    return node.Update( node.OpenBraceToken, List( newStatements ), node.CloseBraceToken );
                }
                else
                {
                    return node;
                }
            }
        }
    }
}