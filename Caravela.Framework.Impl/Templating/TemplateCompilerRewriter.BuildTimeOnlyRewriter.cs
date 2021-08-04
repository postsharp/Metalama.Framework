// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Caravela.Framework.Impl.Templating
{
    internal sealed partial class TemplateCompilerRewriter
    {
        private class BuildTimeOnlyRewriter : CSharpSyntaxRewriter
        {
            private readonly TemplateCompilerRewriter _rewriter;

            public BuildTimeOnlyRewriter( TemplateCompilerRewriter rewriter )
            {
                this._rewriter = rewriter;
            }

            public override SyntaxNode? VisitIdentifierName( IdentifierNameSyntax node )
            {
                if ( this._rewriter._templateMemberClassifier.GetMetaMemberKind( node ) == MetaMemberKind.Proceed )
                {
                    return node.WithIdentifier( SyntaxFactory.Identifier( nameof(meta.Proceed) ) );
                }
                else
                {
                    return node;
                }
            }
        }
    }
}