// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Utilities.Caching;
using Microsoft.CodeAnalysis;
using System.Text;

namespace Metalama.Framework.Engine.Utilities.Roslyn
{
    internal static class NamespaceHelper
    {
        private static readonly WeakCache<INamespaceOrTypeSymbol, string?> _fullNameCache = new();
        private static readonly WeakCache<INamespaceOrTypeSymbol, string?> _fullMetadataCache = new();

        public static string? GetFullName( this INamespaceOrTypeSymbol? symbol )
            => symbol == null ? null : _fullNameCache.GetOrAdd( symbol, s => GetFullName( s, '.', false ) );

        public static string GetFullMetadataName( this INamedTypeSymbol symbol ) => ((INamespaceOrTypeSymbol) symbol).GetFullMetadataName()!;

        public static string? GetFullMetadataName( this INamespaceOrTypeSymbol? symbol )
            => symbol == null ? null : _fullMetadataCache.GetOrAdd( symbol, s => GetFullName( s, '+', true ) );

        private static string? GetFullName( this INamespaceOrTypeSymbol? symbol, char nestedTypeSeparator, bool useMetadataName )
        {
            if ( symbol is null or INamespaceSymbol { IsGlobalNamespace: true } )
            {
                return null;
            }

            var stringBuilder = new StringBuilder();

            void AppendNameRecursive( ISymbol s )
            {
                var (parent, separator, arity) = s switch
                {
                    INamedTypeSymbol { ContainingType: { } } namedType => (namedType.ContainingType, nestedTypeSeparator, namedType.Arity),
                    INamedTypeSymbol namedType => (namedType.ContainingNamespace, '.', namedType.Arity),
                    INamespaceSymbol ns => (ns.ContainingNamespace, '.', 0),
                    _ => (s.ContainingSymbol, '.', 0)
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

                if ( useMetadataName && arity > 0 )
                {
                    stringBuilder.Append( '`' );
                    stringBuilder.Append( arity );
                }
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