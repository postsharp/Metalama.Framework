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
        private static bool IsRootNamespace( ISymbol symbol ) => symbol is INamespaceSymbol ns && ns.IsGlobalNamespace;

        public static AssemblyIdentity ToAssemblyIdentity( this AssemblyName assemblyName ) => new( assemblyName.Name, assemblyName.Version );

        public static string GetReflectionNameSafe( this ISymbol? s )
            => GetReflectionName( s ) ?? throw new ArgumentOutOfRangeException( $"Cannot get a reflection name for {s}." );

        public static INamedTypeSymbol GetTypeByMetadataNameSafe( this Compilation compilation, string name )
            => compilation.GetTypeByMetadataName( name ) ?? throw new ArgumentOutOfRangeException(
                nameof(name),
                $"Cannot find a type '{name}' in compilation '{compilation.AssemblyName}" );

        public static string? GetReflectionName( this ISymbol? s )
        {
            if ( s == null || IsRootNamespace( s ) )
            {
                return string.Empty;
            }

            if ( s is IErrorTypeSymbol error )
            {
                throw new ArgumentOutOfRangeException( nameof(s), $"Cannot get the reflection name of the unresolved symbol '{error.Name}'." );
            }

            var sb = new StringBuilder();

            bool TryFormat( ISymbol symbol )
            {
                switch ( symbol.ContainingSymbol )
                {
                    case null:
                        break;

                    case ITypeSymbol typeSymbol:
                        if ( !TryFormat( typeSymbol ) )
                        {
                            return false;
                        }

                        sb.Append( '+' );

                        break;

                    case INamespaceSymbol namespaceSymbol:
                        if ( !namespaceSymbol.IsGlobalNamespace )
                        {
                            if ( !TryFormat( namespaceSymbol ) )
                            {
                                return false;
                            }

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
                            if ( !TryFormat( arrayTypeSymbol.ElementType ) )
                            {
                                return false;
                            }

                            sb.Append( "[]" );

                            break;

                        case ITypeSymbol type when type.IsAnonymousType:
                            return false;

                        default:
                            throw new NotImplementedException( $"Don't know how to get the reflection name of '{symbol}'." );
                    }
                }

                return true;
            }

            if ( !TryFormat( s ) )
            {
                return null;
            }

            return sb.ToString();
        }
    }
}