// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Microsoft.CodeAnalysis;
using System.Text;

namespace Metalama.Framework.Engine.Utilities.Roslyn
{
    internal static class NamespaceHelper
    {
        public static string? GetFullName( this ISymbol? symbol ) => GetFullName( symbol, '.' );

        public static string? GetFullMetadataName( this ISymbol? symbol ) => GetFullName( symbol, '+' );

        private static string? GetFullName( this ISymbol? symbol, char nestedTypeSeparator )
        {
            if ( symbol == null || symbol is INamespaceSymbol { IsGlobalNamespace: true } )
            {
                return null;
            }

            var stringBuilder = new StringBuilder();

            void AppendNameRecursive( ISymbol s )
            {
                var (parent, separator) = s switch
                {
                    INamedTypeSymbol { ContainingType: { } } namedType => (namedType.ContainingType, nestedTypeSeparator),
                    INamedTypeSymbol namedType => (namedType.ContainingNamespace, '.'),
                    INamespaceSymbol ns => (ns.ContainingNamespace, '.'),
                    _ => (s.ContainingSymbol, '.')
                };

                if ( parent != null )
                {
                    AppendNameRecursive( parent );
                }

                if ( stringBuilder.Length > 0 )
                {
                    stringBuilder.Append( separator );
                }

                stringBuilder.Append( s.Name );
            }

            AppendNameRecursive( symbol );

            return stringBuilder.ToString();
        }

        /// <summary>
        /// Returns the left part before the last '.' of a string.
        /// </summary>
        public static string GetNamespace( string fullName )
        {
            var index = fullName.LastIndexOf( '.' );

            if ( index >= 0 )
            {
                return fullName.Substring( 0, index - 1 );
            }
            else
            {
                return "";
            }
        }
    }
}