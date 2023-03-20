// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Utilities.Caching;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Metalama.Framework.Engine.Utilities.Roslyn
{
    internal static class ReflectionHelper
    {
        // From internal System.TypeNameKind in System.Private.CoreLib
        private enum TypeNameKind
        {
            Name,
            ToString,
            FullName,
        }

        private static readonly WeakCache<INamespaceOrTypeSymbol, string?> _reflectionNameCache = new();
        private static readonly WeakCache<INamespaceOrTypeSymbol, string?> _reflectionFullNameCache = new();
        private static readonly WeakCache<INamespaceOrTypeSymbol, string?> _reflectionToStringNameCache = new();

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

        public static IAssemblySymbol? GetAssembly( this Compilation compilation, AssemblyIdentity assemblyName )
        {
            if ( compilation.Assembly.Identity.Equals( assemblyName ) )
            {
                return compilation.Assembly;
            }
            else
            {
                return compilation.SourceModule.ReferencedAssemblySymbols.FirstOrDefault( a => a.Identity.Equals( assemblyName ) );
            }
        }

        public static IAssemblySymbol? GetAssembly( this Compilation compilation, string assemblyName )
        {
            if ( compilation.Assembly.Name == assemblyName )
            {
                return compilation.Assembly;
            }
            else
            {
                return compilation.SourceModule.ReferencedAssemblySymbols.FirstOrDefault( a => a.Name == assemblyName );
            }
        }

        /// <summary>
        /// Gets a string that would be equal to <see cref="MemberInfo.Name"/>.
        /// </summary>
        public static string GetReflectionName( this INamedTypeSymbol symbol )
            => ((INamespaceOrTypeSymbol) symbol).GetReflectionName()!;

        /// <summary>
        /// Gets a string that would be equal to <see cref="MemberInfo.Name"/>.
        /// </summary>
        public static string? GetReflectionName( this INamespaceOrTypeSymbol s )
            => _reflectionNameCache.GetOrAdd( s, s => s.GetReflectionName( TypeNameKind.Name ) );

        /// <summary>
        /// Gets a string that would be equal to <see cref="Type.FullName"/>, except that we do not qualify type names with the assembly name.
        /// </summary>
        public static string GetReflectionFullName( this INamedTypeSymbol s )
            => ((INamespaceOrTypeSymbol) s).GetReflectionFullName()!;

        /// <summary>
        /// Gets a string that would be equal to <see cref="Type.FullName"/>, except that we do not qualify type names with the assembly name.
        /// </summary>
        public static string? GetReflectionFullName( this INamespaceOrTypeSymbol s )
            => _reflectionFullNameCache.GetOrAdd( s, s => s.GetReflectionName( TypeNameKind.FullName ) );

        /// <summary>
        /// Gets a string that would be equal to the returned value of <see cref="Type.ToString"/> method.
        /// </summary>
        public static string GetReflectionToStringName( this INamedTypeSymbol s )
            => ((INamespaceOrTypeSymbol) s).GetReflectionToStringName()!;

        /// <summary>
        /// Gets a string that would be equal to the returned value of <see cref="Type.ToString"/> method.
        /// </summary>
        public static string? GetReflectionToStringName( this INamespaceOrTypeSymbol s )
            => _reflectionToStringNameCache.GetOrAdd( s, s => s.GetReflectionName( TypeNameKind.ToString ) );

        private static string? GetReflectionName( this INamespaceOrTypeSymbol s, TypeNameKind kind )
        {
            if ( s is ITypeParameterSymbol typeParameter )
            {
                return typeParameter.Name;
            }

            var sb = new StringBuilder();

            if ( !TryAppend( s ) )
            {
                return null;
            }

            return sb.ToString();

            bool TryAppend( INamespaceOrTypeSymbol symbol, List<ITypeSymbol>? typeArguments = null )
            {
                var currentTypeArguments = typeArguments ?? new();

                // Append the containing namespace or type.
                if ( kind != TypeNameKind.Name && symbol is not ITypeParameterSymbol )
                {
                    switch ( symbol.ContainingSymbol )
                    {
                        case null:
                            break;

                        case ITypeSymbol type:
                            if ( !TryAppend( type, currentTypeArguments ) )
                            {
                                return false;
                            }

                            sb.Append( '+' );

                            break;

                        case INamespaceSymbol ns:
                            if ( !ns.IsGlobalNamespace )
                            {
                                if ( !TryAppend( ns ) )
                                {
                                    return false;
                                }

                                sb.Append( '.' );
                            }

                            break;

                        default:
                            // A type is always contained in another type or in a namespace, possibly the global namespace.
                            throw new AssertionFailedException( $"'{symbol}' has an unexpected containing symbol kind {symbol.ContainingSymbol.Kind}." );
                    }
                }

                switch ( symbol )
                {
                    case INamedTypeSymbol { IsGenericType: true } unboundGenericType
                        when (!unboundGenericType.IsGenericTypeDefinition() && kind != TypeNameKind.Name)
                             || kind == TypeNameKind.ToString:
                        sb.Append( unboundGenericType.MetadataName );

                        currentTypeArguments.AddRange( unboundGenericType.TypeArguments );

                        break;

                    case { } when !string.IsNullOrEmpty( symbol.MetadataName ):
                        sb.Append( symbol.MetadataName );

                        break;

                    case IArrayTypeSymbol array:
                        if ( !TryAppend( array.ElementType ) )
                        {
                            return false;
                        }

                        sb.Append( '[' );

                        for ( var i = 1; i < array.Rank; i++ )
                        {
                            sb.Append( ',' );
                        }

                        sb.Append( ']' );

                        break;

                    case IPointerTypeSymbol pointer:
                        if ( !TryAppend( pointer.PointedAtType ) )
                        {
                            return false;
                        }

                        sb.Append( '*' );

                        break;

                    case IDynamicTypeSymbol:
                        sb.Append( "System.Object" );

                        break;

                    case IErrorTypeSymbol:
                        return false;

                    default:
                        throw new AssertionFailedException( $"Don't know how to process a {symbol!.Kind}." );
                }

                if ( typeArguments == null && currentTypeArguments.Any() )
                {
                    sb.Append( '[' );

                    for ( var i = 0; i < currentTypeArguments.Count; i++ )
                    {
                        if ( i > 0 )
                        {
                            sb.Append( ',' );
                        }

                        var arg = currentTypeArguments[i];

                        if ( !TryAppend( arg ) )
                        {
                            return false;
                        }
                    }

                    sb.Append( ']' );
                }

                return true;
            }
        }

#pragma warning disable SA1629 // Documentation text should end with a period
        /// <summary>
        /// Gets a properly-escaped assembly-qualified type name from its components.
        /// </summary>
        /// <param name="typeName">The type name.</param>
        /// <param name="assemblyName">The assembly name.</param>
        /// <returns>A string of the form <code>TypeName, AssemblyName</code>, where commas in <paramref name="typeName"/> have been properly escaped.</returns>
        public static string GetAssemblyQualifiedTypeName( string typeName, string assemblyName )
#pragma warning restore SA1629 // Documentation text should end with a period
        {
            if ( typeName == null )
            {
                throw new ArgumentNullException( nameof(typeName) );
            }

            if ( assemblyName == null )
            {
                throw new ArgumentNullException( nameof(assemblyName) );
            }

            return typeName.ReplaceOrdinal( ",", "\\," ) + ", " + assemblyName;
        }
    }
}