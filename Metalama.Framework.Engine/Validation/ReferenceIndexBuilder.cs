﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Validation;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.Validation;

internal abstract class ReferenceIndexBuilder
{
    public void AddReference( ISymbol? referencedSymbol, ISymbol? referencingSymbol, SyntaxNodeOrToken node, ReferenceKinds referenceKind )
    {
        if ( referencedSymbol == null || referencingSymbol == null )
        {
            return;
        }

        referencedSymbol = referencedSymbol.OriginalDefinition;
        referencingSymbol = referencingSymbol.OriginalDefinition;

        if ( !CheckSymbolKind( referencedSymbol ) || !CheckSymbolKind( referencingSymbol ) )
        {
            return;
        }

        this.AddReferenceCore( referencedSymbol, referencingSymbol, node, referenceKind );
    }

    protected abstract void AddReferenceCore( ISymbol referencedSymbol, ISymbol referencingSymbol, SyntaxNodeOrToken node, ReferenceKinds referenceKind );

    private static bool CheckSymbolKind( ISymbol symbol )
    {
        switch ( symbol.Kind )
        {
            case SymbolKind.Local:
            case SymbolKind.Alias:
            case SymbolKind.Label:
            case SymbolKind.Preprocessing:
            case SymbolKind.DynamicType:
                return false;
        }

        if ( symbol is IMethodSymbol { MethodKind: MethodKind.LocalFunction } )
        {
            return false;
        }

        return true;
    }
}