// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.CodeModel.References;

internal static class SymbolNormalizer
{
    public static IMethodSymbol GetCanonicalSymbol( IMethodSymbol methodSymbol )
    {
        if ( methodSymbol.PartialImplementationPart != null )
        {
            methodSymbol = methodSymbol.PartialImplementationPart;
        }

        if ( IsCanonicalGenericMethodInstance( methodSymbol ) )
        {
            return methodSymbol.OriginalDefinition;
        }
        else
        {
            return methodSymbol;
        }
    }

    public static INamedTypeSymbol GetCanonicalSymbol( INamedTypeSymbol namedTypeSymbol )
    {
        if ( IsCanonicalGenericTypeInstance( namedTypeSymbol ) )
        {
            return namedTypeSymbol.OriginalDefinition;
        }
        else
        {
            return namedTypeSymbol;
        }
    }

    public static ISymbol GetCanonicalSymbol( ISymbol symbol )
    {
        switch ( symbol.Kind )
        {
            case SymbolKind.Method:
                return GetCanonicalSymbol( (IMethodSymbol) symbol );

            case SymbolKind.NamedType:
                return GetCanonicalSymbol( (INamedTypeSymbol) symbol );

            default:
                return symbol;
        }
    }

    private static bool IsCanonicalGenericTypeInstance( INamedTypeSymbol namedTypeSymbol )
    {
        if ( namedTypeSymbol.IsDefinitionSafe() )
        {
            return false;
        }
        else
        {
            for ( var i = 0; i < namedTypeSymbol.TypeArguments.Length; i++ )
            {
                if ( namedTypeSymbol.TypeArguments[i] is not ITypeParameterSymbol { TypeParameterKind: TypeParameterKind.Type } typeParameterSymbol
                     || typeParameterSymbol.Ordinal != i )
                {
                    return false;
                }
            }

            if ( namedTypeSymbol.ContainingType != null && !IsCanonicalGenericTypeInstance( namedTypeSymbol.ContainingType ) )
            {
                return false;
            }

            return true;
        }
    }

    private static bool IsCanonicalGenericMethodInstance( IMethodSymbol methodSymbol )
    {
        if ( methodSymbol.IsDefinitionSafe() )
        {
            return false;
        }
        else
        {
            for ( var i = 0; i < methodSymbol.TypeArguments.Length; i++ )
            {
                if ( methodSymbol.TypeArguments[i] is not ITypeParameterSymbol { TypeParameterKind: TypeParameterKind.Method } typeParameterSymbol
                     || typeParameterSymbol.Ordinal != i )
                {
                    return false;
                }
            }

            if ( !IsCanonicalGenericTypeInstance( methodSymbol.ContainingType ) )
            {
                return false;
            }

            return true;
        }
    }
}