// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.Linking.Inlining
{
    /// <summary>
    /// Allows for one kind of inlining of aspect references.
    /// </summary>
    internal abstract class Inliner
    {
        /// <summary>
        /// Determines whether the inliner can be used for the specified target symbol.
        /// </summary>
        /// <param name="symbol">Target symbol.</param>
        /// <returns></returns>
        public abstract bool IsValidForTargetSymbol( ISymbol symbol );

        /// <summary>
        /// Determines whether the inliner can be used for the specified containing symbol.
        /// </summary>
        /// <param name="symbol">Containing symbol.</param>
        /// <returns></returns>
        public abstract bool IsValidForContainingSymbol( ISymbol symbol );

        /// <summary>
        /// Determines whether an aspect reference can be inlined.
        /// </summary>
        /// <param name="aspectReference">Resolved aspect reference.</param>
        /// <returns></returns>
        public virtual bool CanInline( ResolvedAspectReference aspectReference, SemanticModel semanticModel )
        {
            if ( !SymbolEqualityComparer.Default.Equals(aspectReference.ContainingSymbol.ContainingType, aspectReference.ResolvedSemantic.Symbol.ContainingType) )
            {
                return false;
            }

            return true;
        }

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