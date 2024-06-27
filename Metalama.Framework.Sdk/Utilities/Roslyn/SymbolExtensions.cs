using Microsoft.CodeAnalysis;
using System;

namespace Metalama.Framework.Engine.Utilities.Roslyn;

public static class SymbolExtensions
{
    public static SyntaxReference? GetPrimarySyntaxReference( this ISymbol? symbol )
    {
        if ( symbol == null )
        {
            return null;
        }

        static SyntaxReference? GetReferenceOfShortestPath( ISymbol s, Func<SyntaxReference, bool>? filter = null )
        {
            if ( s.DeclaringSyntaxReferences.IsDefaultOrEmpty )
            {
                return null;
            }
            else
            {
                // Find the lowest value.

                SyntaxReference? min = null;
                int? minLength = null;

                foreach ( var reference in s.DeclaringSyntaxReferences )
                {
                    if ( filter != null && !filter( reference ) )
                    {
                        continue;
                    }

                    var length = reference.SyntaxTree.FilePath.Length;

                    if ( min == null || length < minLength )
                    {
                        min = reference;
                        minLength = length;
                    }
                }

                return min;
            }
        }

        switch ( symbol )
        {
            case IMethodSymbol { AssociatedSymbol: not null } methodSymbol:
                return GetReferenceOfShortestPath( symbol ) ?? GetReferenceOfShortestPath( methodSymbol.AssociatedSymbol );

            case IMethodSymbol { IsPartialDefinition: true, PartialImplementationPart: { } partialDefinitionSymbol }:
                return GetReferenceOfShortestPath( partialDefinitionSymbol );

            case IMethodSymbol { IsPartialDefinition: true, PartialImplementationPart: null }:
                return GetReferenceOfShortestPath( symbol );

            default:
                return GetReferenceOfShortestPath( symbol );
        }
    }

    public static SyntaxNode? GetPrimaryDeclaration( this ISymbol symbol ) => symbol.GetPrimarySyntaxReference()?.GetSyntax();
}