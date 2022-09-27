// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.Linking.Substitution
{
    internal class RootWithUsingLocalSubstitution : SyntaxNodeSubstitution
    {
        public BlockSyntax RootBlock { get; }

        public RootWithUsingLocalSubstitution( BlockSyntax rootBlock )
        {
            this.RootBlock = rootBlock;
        }

        public override SyntaxNode TargetNode => this.RootBlock;

        public override SyntaxNode? Substitute( SyntaxNode currentNode, SubstitutionContext substitutionContext )
        {
            switch ( currentNode )
            {
                case BlockSyntax rootBlock:
                    LocalDeclarationStatementSyntax? firstUsingLocalStatement = null;
                    var initialStatements = new List<StatementSyntax>();
                    var tailStatements = new List<StatementSyntax>();

                    foreach ( var statement in rootBlock.Statements )
                    {
                        if ( firstUsingLocalStatement == null )
                        {
                            if ( statement is LocalDeclarationStatementSyntax local && local.UsingKeyword != default )
                            {
                                firstUsingLocalStatement = local;
                            }
                            else
                            {
                                initialStatements.Add( statement );
                            }
                        }
                        else
                        {
                            tailStatements.Add( statement );
                        }
                    }

                    if ( firstUsingLocalStatement == null )
                    {
                        return currentNode;
                    }
                    else
                    {
                        initialStatements.Add( Translate( firstUsingLocalStatement, tailStatements ) );

                        return rootBlock.WithStatements( List( initialStatements ) );
                    }

                default:
                    throw new AssertionFailedException();
            }

            UsingStatementSyntax Translate( LocalDeclarationStatementSyntax local, List<StatementSyntax> statements )
            {
                return
                    UsingStatement(
                        local.Declaration,
                        null,
                        Block( statements ) );
            }
        }
    }
}