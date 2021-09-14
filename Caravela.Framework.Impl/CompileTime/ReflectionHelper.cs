// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using System;
using System.Reflection;
using System.Text;

namespace Caravela.Framework.Impl.CompileTime
{
    internal static class ReflectionHelper
    {
        public static AssemblyIdentity ToAssemblyIdentity( this AssemblyName assemblyName ) => new( assemblyName.Name, assemblyName.Version );

        public static INamedTypeSymbol GetTypeByMetadataNameSafe( this Compilation compilation, string name )
            => compilation.GetTypeByMetadataName( name ) ?? throw new ArgumentOutOfRangeException(
                nameof(name),
                $"Cannot find a type '{name}' in compilation '{compilation.AssemblyName}" );

        public static string GetReflectionName( this INamedTypeSymbol s )
        {
            if ( s is IErrorTypeSymbol error )
            {
                throw new ArgumentOutOfRangeException( nameof(s), $"Cannot get the reflection name of the unresolved symbol '{error.Name}'." );
            }

            var sb = new StringBuilder();
            Append( s );

            void Append( INamespaceOrTypeSymbol symbol )
            {
                switch ( symbol.ContainingSymbol )
                {
                    case ITypeSymbol typeSymbol:
                        Append( typeSymbol );
                        sb.Append( '+' );

                        break;

                    case INamespaceSymbol namespaceSymbol:
                        if ( !namespaceSymbol.IsGlobalNamespace )
                        {
                            Append( namespaceSymbol );
                            sb.Append( '.' );
                        }

                        break;

                    default:
                        // A type is always contained in another type or in a namespace, possibly the global namespace.
                        throw new AssertionFailedException();
                }

                if ( !string.IsNullOrEmpty( symbol.MetadataName ) )
                {
                    sb.Append( symbol.MetadataName );
                }
                else
                {
                    throw new AssertionFailedException( $"{symbol.ToDisplayString()} does not have a MetadataName." );
                }
            }

            return sb.ToString();
        }
    }
}