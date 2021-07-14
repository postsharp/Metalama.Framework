// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.Linking.Inlining
{
    /// <summary>
    /// Allows for one kind of inlining of aspect references.
    /// </summary>
    internal abstract class Inliner
    {
        public abstract IReadOnlyList<SyntaxKind> AncestorSyntaxKinds { get; }

        /// <summary>
        /// Determines whether an aspect reference can be inlined.
        /// </summary>
        /// <param name="aspectReference">Resolved aspect reference.</param>
        /// <param name="semanticModel">Semantic model for the syntax tree containing the expression.</param>
        /// <returns></returns>
        public abstract bool CanInline( ResolvedAspectReference aspectReference, SemanticModel semanticModel );

        /// <summary>
        /// Inlines the target of the annotated expression by specifying node to be replaced and the replacing node.
        /// </summary>
        /// <param name="context">Inlining context.</param>
        /// <param name="aspectReference">Aspect reference.</param>
        /// <param name="replacedNode">Replaced node (has to be direct ancestor of the annotated expression).</param>
        /// <param name="newNode"></param>
        public abstract void Inline( InliningContext context, ResolvedAspectReference aspectReference, out SyntaxNode replacedNode, out SyntaxNode newNode );
    }
}