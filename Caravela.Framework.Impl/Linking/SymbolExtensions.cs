// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using System.Linq;

namespace Caravela.Framework.Impl.Linking
{
    public static class SymbolExtensions
    {
        // TODO: Partial methods etc.

        public static SyntaxReference? GetPrimarySyntaxReference( this ISymbol symbol )
        {
            switch ( symbol )
            {
                case IMethodSymbol { AssociatedSymbol: not null } methodSymbol:
                    return symbol.DeclaringSyntaxReferences.OrderBy( x => x.SyntaxTree.FilePath.Length ).FirstOrDefault()
                           ?? methodSymbol.AssociatedSymbol!.DeclaringSyntaxReferences.OrderBy( x => x.SyntaxTree.FilePath.Length ).FirstOrDefault();

                default:
                    return symbol.DeclaringSyntaxReferences.OrderBy( x => x.SyntaxTree.FilePath.Length ).FirstOrDefault();
            }
        }

        public static SyntaxNode? GetPrimaryDeclaration( this ISymbol symbol )
        {
            switch ( symbol )
            {
                case IMethodSymbol { AssociatedSymbol: not null } methodSymbol:
                    return symbol.DeclaringSyntaxReferences.OrderBy( x => x.SyntaxTree.FilePath.Length ).FirstOrDefault()?.GetSyntax()
                           ?? methodSymbol.AssociatedSymbol!.DeclaringSyntaxReferences.OrderBy( x => x.SyntaxTree.FilePath.Length )
                               .FirstOrDefault()
                               ?.GetSyntax();

                default:
                    return symbol.DeclaringSyntaxReferences.OrderBy( x => x.SyntaxTree.FilePath.Length ).FirstOrDefault()?.GetSyntax();
            }
        }
    }
}