// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.Linking
{
    internal partial class LinkerLinkingStep
    {
        private class CleanupRewriter : CSharpSyntaxRewriter
        {
            public override SyntaxNode? VisitMethodDeclaration( MethodDeclarationSyntax node )
            {
                return 
                    node
                    .WithBody( RewriteBodyBlock( node.Body ) );
            }

            public override SyntaxNode? VisitPropertyDeclaration( PropertyDeclarationSyntax node )
            {
                return VisitBasePropertyDeclaration( node );
            }

            public override SyntaxNode? VisitIndexerDeclaration( IndexerDeclarationSyntax node )
            {
                return VisitBasePropertyDeclaration( node );
            }

            public override SyntaxNode? VisitEventDeclaration( EventDeclarationSyntax node )
            {
                return VisitBasePropertyDeclaration( node );
            }

            private static SyntaxNode? VisitBasePropertyDeclaration( BasePropertyDeclarationSyntax node )
            {
                return
                    node.WithAccessorList(
                        node.AccessorList?.WithAccessors(
                            List(
                                node.AccessorList.Accessors
                                    .Select( a => a.WithBody( RewriteBodyBlock( a.Body ) ) ) ) ) );
            }

            private static BlockSyntax? RewriteBodyBlock(BlockSyntax? block)
            {
                if ( block == null )
                {
                    return null;
                }
                else
                {
                    var countLabelUsesWalker = new CountLabelUsesWalker();
                    countLabelUsesWalker.Visit( block );
                    return 
                        (BlockSyntax?)
                        new RemoveTrailingReturnRewriter().Visit(
                            new RemoveTrivialLabelRewriter(countLabelUsesWalker.ObservedLabelCounters).Visit(
                                new CleanupBodyRewriter().Visit(block) ) );
                }
            }
        }
    }
}