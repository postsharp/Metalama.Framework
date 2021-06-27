// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Caravela.Framework.Impl.Linking
{
    internal class AspectReferenceHandle
    {
        public ISymbol ContainingSymbol { get; }

        public ISymbol ReferencedSymbol { get; }

        public ExpressionSyntax Expression { get; }

        public AspectReferenceSpecification Specification { get; }

        public AspectReferenceHandle(ISymbol containingSymbol, ISymbol referencedSymbol, ExpressionSyntax expression, AspectReferenceSpecification specification )
        {
            this.ContainingSymbol = containingSymbol;
            this.ReferencedSymbol = referencedSymbol;
            this.Expression = expression;
            this.Specification = specification;
        }
    }
}