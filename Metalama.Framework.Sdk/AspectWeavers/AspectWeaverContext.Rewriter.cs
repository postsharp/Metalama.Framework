// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.AspectWeavers
{
    public sealed partial class AspectWeaverContext
    {
        private class Rewriter : SafeSyntaxRewriter
        {
            private readonly ImmutableHashSet<SyntaxNode> _targets;
            private readonly CSharpSyntaxRewriter _userRewriter;

            public Rewriter( ImmutableHashSet<SyntaxNode> targets, CSharpSyntaxRewriter userRewriter )
            {
                this._userRewriter = userRewriter;
                this._targets = targets;
            }

            protected override SyntaxNode? VisitCore( SyntaxNode? node )
            {
                switch ( node )
                {
                    case CompilationUnitSyntax:
                        return base.VisitCore( node );

                    case MemberDeclarationSyntax or AccessorDeclarationSyntax:
                        {
                            if ( this._targets.Contains( node ) )
                            {
                                return this._userRewriter.Visit( node );
                            }
                            else if ( node is BaseTypeDeclarationSyntax or NamespaceDeclarationSyntax )
                            {
                                // Visit types and namespaces.

                                return base.VisitCore( node );
                            }

                            break;
                        }
                }

                // Don't visit other members.
                return node;
            }
        }
    }
}