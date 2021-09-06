// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Caravela.Framework.Impl.Linking
{
    internal class ResolvedAspectReference
    {
        /// <summary>
        /// Gets the symbol that contains the reference.
        /// </summary>
        public ISymbol ContainingSymbol { get; }

        /// <summary>
        /// Gets the symbol the reference was originally pointing to.
        /// </summary>
        public ISymbol OriginalSymbol { get; }

        /// <summary>
        /// Gets the symbol semantic that is the target of the reference.
        /// </summary>
        public IntermediateSymbolSemantic ResolvedSemantic { get; }

        /// <summary>
        /// Gets the annotated expression.
        /// </summary>
        public ExpressionSyntax Expression { get; }

        /// <summary>
        /// Gets the original specification of the reference.
        /// </summary>
        public AspectReferenceSpecification Specification { get; } // TODO: Remove, all information should be translated.

        public ResolvedAspectReference(
            ISymbol containingSymbol,
            ISymbol originalSymbol,
            IntermediateSymbolSemantic resolvedSemantic,
            ExpressionSyntax expression,
            AspectReferenceSpecification specification )
        {
            this.ContainingSymbol = containingSymbol;
            this.OriginalSymbol = originalSymbol;
            this.ResolvedSemantic = resolvedSemantic;
            this.Expression = expression;
            this.Specification = specification;
        }
    }
}