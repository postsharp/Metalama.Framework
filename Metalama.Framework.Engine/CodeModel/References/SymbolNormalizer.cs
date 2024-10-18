// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.GenericContexts;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;
using TypeParameterKind = Microsoft.CodeAnalysis.TypeParameterKind;

namespace Metalama.Framework.Engine.CodeModel.References;

internal static class SymbolNormalizer
{
    private static (ISymbol Symbol, GenericContext Context) GetCanonicalSymbol(
        IMethodSymbol methodSymbol,
        GenericContext genericContext,
        RefFactory refFactory )
    {
        if ( methodSymbol.PartialImplementationPart != null )
        {
            methodSymbol = methodSymbol.PartialImplementationPart;
        }

        if ( GenericContextHelper.IsCanonicalGenericMethodInstance( methodSymbol ) )
        {
            return (methodSymbol.OriginalDefinition, genericContext);
        }
        else if ( genericContext.IsEmptyOrIdentity )
        {
            return (methodSymbol, genericContext);
        }
        else
        {
            return (methodSymbol.OriginalDefinition, SymbolGenericContext.Get( methodSymbol, refFactory.CompilationContext ).Map( genericContext, refFactory ));
        }
    }

    private static (ISymbol Symbol, GenericContext Context) GetCanonicalSymbol(
        INamedTypeSymbol namedTypeSymbol,
        GenericContext genericContext,
        RefFactory refFactory )
    {
        if ( GenericContextHelper.IsCanonicalGenericTypeInstance( namedTypeSymbol ) )
        {
            var definition = namedTypeSymbol.OriginalDefinition.WithNullableAnnotation( namedTypeSymbol.NullableAnnotation );

            return (definition, genericContext);
        }
        else if ( genericContext.IsEmptyOrIdentity )
        {
            return (namedTypeSymbol, genericContext);
        }
        else
        {
            return (namedTypeSymbol.OriginalDefinition,
                    SymbolGenericContext.Get( namedTypeSymbol, refFactory.CompilationContext ).Map( genericContext, refFactory ));
        }
    }

    public static (ISymbol Symbol, GenericContext Context) GetCanonicalSymbol( ISymbol symbol, GenericContext genericContext, RefFactory refFactory )
    {
        switch ( symbol.Kind )
        {
            case SymbolKind.Method:
                return GetCanonicalSymbol( (IMethodSymbol) symbol, genericContext, refFactory );

            case SymbolKind.NamedType:
                return GetCanonicalSymbol( (INamedTypeSymbol) symbol, genericContext, refFactory );

            default:
                return (symbol, genericContext);
        }
    }
}