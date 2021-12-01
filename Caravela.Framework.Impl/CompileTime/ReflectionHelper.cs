// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Utilities;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;
using System.Reflection;
using System.Text;

namespace Caravela.Framework.Impl.CompileTime
{
    internal static class ReflectionHelper
    {
        public static AssemblyIdentity ToAssemblyIdentity( this AssemblyName assemblyName )
        {
            ImmutableArray<byte> publicKeyOrToken = default;
            var hasPublicKey = false;

            var publicKey = assemblyName.GetPublicKey();

            if ( publicKey != null )
            {
                publicKeyOrToken = publicKey.ToImmutableArray();
                hasPublicKey = true;
            }
            else
            {
                var publicKeyToken = assemblyName.GetPublicKeyToken();

                if ( publicKeyToken != null )
                {
                    publicKeyOrToken = publicKeyToken.ToImmutableArray();
                }
            }

            return new AssemblyIdentity(
                assemblyName.Name,
                assemblyName.Version,
                assemblyName.CultureName,
                publicKeyOrToken,
                hasPublicKey );
        }

        public static INamedTypeSymbol GetTypeByMetadataNameSafe( this Compilation compilation, string name )
            => compilation.GetTypeByMetadataName( name ) ?? throw new ArgumentOutOfRangeException(
                nameof(name),
                $"Cannot find a type '{name}' in compilation '{compilation.AssemblyName}" );

        /// <summary>
        /// Gets a string that would be equal to <see cref="Type.FullName"/>, except that we do not qualify type names with the assembly name.
        /// </summary>
        public static string GetReflectionName( this ITypeSymbol s )
        {
            if ( s is ITypeParameterSymbol typeParameter )
            {
                return typeParameter.Name;
            }

            var sb = new StringBuilder();
            Append( s );

            void Append( INamespaceOrTypeSymbol symbol )
            {
                // Append the containing namespace or type.
                switch ( symbol.ContainingSymbol )
                {
                    case null:
                        break;

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

                switch ( symbol )
                {
                    case INamedTypeSymbol { IsGenericType: true } unboundGenericType when !unboundGenericType.IsGenericTypeDefinition():
                        sb.Append( unboundGenericType.MetadataName );
                        sb.Append( "[" );

                        for ( var i = 0; i < unboundGenericType.TypeArguments.Length; i++ )
                        {
                            if ( i > 0 )
                            {
                                sb.Append( ", " );
                            }

                            var arg = unboundGenericType.TypeArguments[i];
                            Append( arg );
                        }

                        sb.Append( "]" );

                        break;

                    case { } when !string.IsNullOrEmpty( symbol.MetadataName ):
                        sb.Append( symbol.MetadataName );

                        break;

                    case IArrayTypeSymbol array:
                        Append( array.ElementType );
                        sb.Append( '[' );

                        for ( var i = 1; i < array.Rank; i++ )
                        {
                            sb.Append( ',' );
                        }

                        sb.Append( ']' );

                        break;

                    case IPointerTypeSymbol pointer:
                        Append( pointer.PointedAtType );
                        sb.Append( '*' );

                        break;

                    case IDynamicTypeSymbol:
                        sb.Append( "System.Object" );

                        break;

                    default:
                        throw new AssertionFailedException( $"Don't know how to process a {symbol!.Kind}." );
                }
            }

            return sb.ToString();
        }
    }
}