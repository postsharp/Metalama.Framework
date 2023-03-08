// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Utilities.Caching;
using Microsoft.CodeAnalysis;
using System.Text;

namespace Metalama.Framework.Engine.Utilities.Roslyn
{
    internal static class NamespaceHelper
    {
        private static readonly WeakCache<INamespaceOrTypeSymbol, string?> _nameCache = new();
        private static readonly WeakCache<INamespaceOrTypeSymbol, string?> _fullNameCache = new();
        private static readonly WeakCache<INamespaceOrTypeSymbol, string?> _metadataNameCache = new();
        private static readonly WeakCache<INamespaceOrTypeSymbol, string?> _fullMetadataNameCache = new();

        public static string? GetName( this INamespaceOrTypeSymbol? symbol )
            => symbol == null ? null : _nameCache.GetOrAdd( symbol, s => GetName( s, '.', false, false ) );

        public static string? GetFullName( this INamespaceOrTypeSymbol? symbol )
            => symbol == null ? null : _fullNameCache.GetOrAdd( symbol, s => GetName( s, '.', false, true ) );

        public static string GetMetadataName( this INamedTypeSymbol symbol ) => ((INamespaceOrTypeSymbol) symbol).GetMetadataName()!;

        public static string GetFullMetadataName( this INamedTypeSymbol symbol ) => ((INamespaceOrTypeSymbol) symbol).GetFullMetadataName()!;

        private static string? GetMetadataName( this INamespaceOrTypeSymbol? symbol )
            => symbol == null ? null : _metadataNameCache.GetOrAdd( symbol, s => GetName( s, '+', true, false ) );

        private static string? GetFullMetadataName( this INamespaceOrTypeSymbol? symbol )
            => symbol == null ? null : _fullMetadataNameCache.GetOrAdd( symbol, s => GetName( s, '+', true, true ) );

        private static string? GetName( this INamespaceOrTypeSymbol? symbol, char nestedTypeSeparator, bool useMetadataName, bool fullName )
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

                if ( parent != null && (fullName || parent is INamedTypeSymbol) )
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
    }
}