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
            => symbol == null ? null : _fullNameCache.GetOrAdd( symbol, s => GetFullNameImpl( s ) );

        private static string? GetFullNameImpl( this INamespaceOrTypeSymbol? symbol )
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
                    INamedTypeSymbol { ContainingType: { } } namedType => (namedType.ContainingType, '.', namedType.Arity),
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
            }

            AppendNameRecursive( symbol );

            return stringBuilder.ToString();
        }
    }
}