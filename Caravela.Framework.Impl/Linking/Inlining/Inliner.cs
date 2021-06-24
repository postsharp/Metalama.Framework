// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
        /// Determines whether a symbol represented by an annotated expression can be inlined.
        /// </summary>
        /// <param name="contextDeclaration">Symbol that contains the expression.</param>
        /// <param name="semanticModel">Semantic model for the syntax tree containing the expression.</param>
        /// <param name="annotatedExpression">Annotated expression.</param>
        /// <returns></returns>
        public abstract bool CanInline( ISymbol contextDeclaration, SemanticModel semanticModel, ExpressionSyntax annotatedExpression );

        /// <summary>
        /// Inlines the target of the annotated expression by specifying node to be replaced and the replacing node.
        /// </summary>
        /// <param name="context">Inlining context.</param>
        /// <param name="annotatedExpression">Annotated expression.</param>
        /// <param name="replacedNode">Replaced node (has to be direct ancestor of the annotated expression).</param>
        /// <param name="newNode"></param>
        /// <returns></returns>
        public abstract void Inline( InliningContext context, ExpressionSyntax annotatedExpression, out SyntaxNode replacedNode, out SyntaxNode newNode );
    }
}
