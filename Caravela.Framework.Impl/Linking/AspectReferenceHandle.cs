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

        public ExpressionSyntax SyntaxNode { get; }

        public AspectReferenceSpecification Specification { get; }
    }
}