using System;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.CompileTime
{
    internal static class ReflectionNameHelper
    {
        private static bool IsRootNamespace( ISymbol symbol ) => symbol is INamespaceSymbol ns && ns.IsGlobalNamespace;

        public static string GetReflectionName( ISymbol? s )
        {
            if ( s == null || IsRootNamespace( s ) )
            {
                return string.Empty;
            }

            var sb = new StringBuilder();

            void Format( ISymbol symbol )
            {
                switch ( symbol.ContainingSymbol )
                {
                    case null:
                        break;

                    case ITypeSymbol typeSymbol:
                        Format( typeSymbol );
                        sb.Append( '+' );
                        break;

                    case INamespaceSymbol namespaceSymbol:
                        if ( !namespaceSymbol.IsGlobalNamespace )
                        {
                            Format( namespaceSymbol );
                            sb.Append( '.' );
                        }

                        break;

                    default:
                        throw new AssertionFailedException();
                }

                if ( !string.IsNullOrEmpty( symbol.MetadataName ) )
                {
                    sb.Append( symbol.MetadataName );
                }
                else
                {
                    switch ( symbol )
                    {
                        case IArrayTypeSymbol arrayTypeSymbol:
                            Format( arrayTypeSymbol.ElementType );
                            sb.Append( "[]" );
                            break;

                        default:
                            throw new NotImplementedException( $"Don't know how to get the reflection name of '{symbol}'." );
                    }
                }
            }

            Format( s );

            return sb.ToString();
        }
    }
}