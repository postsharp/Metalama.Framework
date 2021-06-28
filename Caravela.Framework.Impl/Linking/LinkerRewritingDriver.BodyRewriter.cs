// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

// TODO: A lot methods here are called multiple times. Optimize.

namespace Caravela.Framework.Impl.Linking
{
    internal partial class LinkerRewritingDriver
    {
        internal class BodyRewriter : CSharpSyntaxRewriter
        {
            private readonly Dictionary<SyntaxNode, SyntaxNode?> _replacements;

            public BodyRewriter( Dictionary<SyntaxNode, SyntaxNode?> replacements )
            {
                // Check that the input is correct - all replaced nodes should be independent in terms of ancestor relation.
                var nodes = new HashSet<SyntaxNode>( replacements.Select( x => x.Key ) );

                // Temporary restriction.                
                foreach (var node in nodes)
                {
                    var current = node.Parent;

                    while (current != null)
                    {
                        if (nodes.Contains(current))
                        {
                            throw new AssertionFailedException();
                        }

                        current = current.Parent;
                    }
                }

                this._replacements = replacements;
            }

            public override SyntaxNode? Visit( SyntaxNode? node )
            {
                if (node == null)
                {
                    return null;
                }
                else if ( this._replacements.TryGetValue( node, out var replacement ) )
                {
                    return replacement;
                }
                else
                {
                    return base.Visit( node );
                }
            }
        }
    }    
}
