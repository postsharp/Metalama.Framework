// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using System.Linq;

namespace Caravela.Framework.Impl.Linking
{
    public static class SymbolExtensions
    {
        public static SyntaxNode? GetPrimaryDeclaration( this ISymbol symbol )
        {
            // TODO: Partials.
            switch ( symbol )
            {
                case IMethodSymbol { AssociatedSymbol: not null } methodSymbol:
                    return symbol.DeclaringSyntaxReferences.SingleOrDefault()?.GetSyntax()
                           ?? methodSymbol.AssociatedSymbol.DeclaringSyntaxReferences.SingleOrDefault()?.GetSyntax();

                default:
                    return symbol.DeclaringSyntaxReferences.SingleOrDefault()?.GetSyntax();
            }
        }
    }
}