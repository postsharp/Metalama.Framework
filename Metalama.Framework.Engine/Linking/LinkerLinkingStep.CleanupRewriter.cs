// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Options;
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
            private readonly IProjectOptions? _projectOptions;

            public CleanupRewriter( IProjectOptions? projectOptions )
            {
                this._projectOptions = projectOptions;
            }

            public override SyntaxNode? VisitMethodDeclaration( MethodDeclarationSyntax node )
            {
                return
                    node
                        .WithBody( this.RewriteBodyBlock( node.Body ) );
            }

            public override SyntaxNode? VisitPropertyDeclaration( PropertyDeclarationSyntax node )
            {
                return this.VisitBasePropertyDeclaration( node );
            }

            public override SyntaxNode? VisitIndexerDeclaration( IndexerDeclarationSyntax node )
            {
                return this.VisitBasePropertyDeclaration( node );
            }

            public override SyntaxNode? VisitEventDeclaration( EventDeclarationSyntax node )
            {
                return this.VisitBasePropertyDeclaration( node );
            }

            private SyntaxNode? VisitBasePropertyDeclaration( BasePropertyDeclarationSyntax node )
            {
                return
                    node.WithAccessorList(
                        node.AccessorList?.WithAccessors(
                            List(
                                node.AccessorList.Accessors
                                    .Select( a => a.WithBody( this.RewriteBodyBlock( a.Body ) ) ) ) ) );
            }

            private BlockSyntax? RewriteBodyBlock( BlockSyntax? block )
            {
                if ( block == null )
                {
                    return null;
                }
                else if ( this._projectOptions?.FormatOutput == true )
                {
                    var countLabelUsesWalker = new CountLabelUsesWalker();
                    countLabelUsesWalker.Visit( block );

                    var withFlattenedBlocks = new CleanupBodyRewriter().Visit( block );
                    var withoutTrivialLabels = new RemoveTrivialLabelRewriter( countLabelUsesWalker.ObservedLabelCounters ).Visit(withFlattenedBlocks);
                    var withoutTrailingReturns = new RemoveTrailingReturnRewriter().Visit( withoutTrivialLabels );

                    return (BlockSyntax?)withoutTrailingReturns;
                }
                else
                {
                    return (BlockSyntax?) new CleanupBodyRewriter().Visit( block );
                }
            }
        }
    }
}