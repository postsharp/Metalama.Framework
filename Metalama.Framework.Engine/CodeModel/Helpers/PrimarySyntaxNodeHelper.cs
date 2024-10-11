// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.Abstractions;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.CodeModel.Helpers
{
    public static class PrimarySyntaxNodeHelper
    {
        internal static SyntaxNode? GetPrimaryDeclarationSyntax( this IDeclaration declaration ) => declaration.GetSymbol()?.GetPrimaryDeclarationSyntax();

        internal static SyntaxNode? GetClosestPrimaryDeclarationSyntax( this IDeclaration declaration )
        {
            for ( var symbol = declaration.GetSymbol(); symbol != null && symbol.Kind != SymbolKind.Namespace; symbol = symbol.ContainingSymbol )
            {
                var syntax = symbol.GetPrimaryDeclarationSyntax();

                if ( syntax != null )
                {
                    return syntax;
                }
            }

            return null;
        }
        
        internal static SyntaxNode? GetClosestPrimaryDeclarationSyntax( this ISymbol symbol )
        {
            for ( ; symbol != null && symbol.Kind != SymbolKind.Namespace; symbol = symbol.ContainingSymbol )
            {
                var syntax = symbol.GetPrimaryDeclarationSyntax();

                if ( syntax != null )
                {
                    return syntax;
                }
            }

            return null;
        }

        internal static SyntaxNode? GetPrimaryDeclarationSyntax( this IFullRef declaration )
            => declaration.GetClosestContainingSymbol().GetPrimaryDeclarationSyntax();

        public static SyntaxTree? GetPrimarySyntaxTree( this IDeclaration declaration ) => ((IDeclarationImpl) declaration).PrimarySyntaxTree;
    }
}