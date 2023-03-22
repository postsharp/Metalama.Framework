// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Utilities.Caching;
using Microsoft.CodeAnalysis;
using System.Text;

namespace Metalama.Framework.Engine.Utilities.Roslyn
{
    internal static class NamespaceHelper
    {
        private static readonly WeakCache<INamespaceOrTypeSymbol, string?> _fullNameCache = new();

        public static string? GetFullName( this INamespaceOrTypeSymbol? symbol )
            => symbol == null ? null : _fullNameCache.GetOrAdd( symbol, GetFullNameImpl );

        private static string? GetFullNameImpl( this INamespaceOrTypeSymbol? symbol )
        {
            if ( symbol is null or INamespaceSymbol { IsGlobalNamespace: true } )
            {
                return null;
            }

            var stringBuilder = new StringBuilder();

            void AppendNameRecursive( ISymbol s )
            {
                var (parent, separator) = s switch
                {
                    INamedTypeSymbol { ContainingType: { } } namedType => (namedType.ContainingType, '.'),
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
    }
}