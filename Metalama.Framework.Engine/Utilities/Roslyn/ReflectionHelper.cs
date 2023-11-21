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
            FullName
        }

        private static readonly WeakCache<INamespaceOrTypeSymbol, string> _reflectionNameCache = new();
        private static readonly WeakCache<INamespaceOrTypeSymbol, string> _reflectionFullNameCache = new();
        private static readonly WeakCache<INamespaceOrTypeSymbol, string> _reflectionToStringNameCache = new();

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
        public static string GetReflectionName( this INamedTypeSymbol symbol ) => ((INamespaceOrTypeSymbol) symbol).GetReflectionName();

        /// <summary>
        /// Gets a string that would be equal to <see cref="MemberInfo.Name"/>.
        /// </summary>
        public static string GetReflectionName( this INamespaceOrTypeSymbol s )
            => _reflectionNameCache.GetOrAdd( s, x => x.GetReflectionName( TypeNameKind.Name ) );

        /// <summary>
        /// Gets a string that would be equal to <see cref="Type.FullName"/>, except that we do not qualify type names with the assembly name.
        /// </summary>
        public static string GetReflectionFullName( this INamedTypeSymbol s ) => ((INamespaceOrTypeSymbol) s).GetReflectionFullName();

        /// <summary>
        /// Gets a string that would be equal to <see cref="Type.FullName"/>, except that we do not qualify type names with the assembly name.
        /// </summary>
        public static string GetReflectionFullName( this INamespaceOrTypeSymbol s )
            => _reflectionFullNameCache.GetOrAdd( s, x => x.GetReflectionName( TypeNameKind.FullName ) );

        /// <summary>
        /// Gets a string that would be equal to the returned value of <see cref="Type.ToString"/> method.
        /// </summary>
        public static string GetReflectionToStringName( this INamespaceOrTypeSymbol s )
            => _reflectionToStringNameCache.GetOrAdd( s, x => x.GetReflectionName( TypeNameKind.ToString ) );

        private static string GetReflectionName( this INamespaceOrTypeSymbol s, TypeNameKind kind )
        {
            if ( s is ITypeParameterSymbol typeParameter )
            {
                return typeParameter.Name;
            }

            var sb = new StringBuilder();

            Append( s );

            return sb.ToString();

            void Append( INamespaceOrTypeSymbol symbol, List<ITypeSymbol>? typeArguments = null )
            {
                var currentTypeArguments = typeArguments ?? [];

                // Append the containing namespace or type.
                if ( kind != TypeNameKind.Name && symbol is not ITypeParameterSymbol )
                {
                    switch ( symbol.ContainingSymbol )
                    {
                        case null:
                            break;

                        case ITypeSymbol type:
                            Append( type, currentTypeArguments );

                            sb.Append( '+' );

                            break;

                        case INamespaceSymbol ns:
                            if ( !ns.IsGlobalNamespace )
                            {
                                Append( ns );

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

                    case IErrorTypeSymbol errorTypeSymbol:
                        // We try to write a name for an unresolved type, even if it is incorrect.
                        // If the caller requires a valid name, it has to verify the type validity differently.
                        sb.Append( errorTypeSymbol.Name );

                        break;

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

                        Append( arg );
                    }

                    sb.Append( ']' );
                }
            }
        }

        /// <summary>
        /// Gets a properly-escaped assembly-qualified type name from its components.
        /// </summary>
        /// <param name="typeName">The type name.</param>
        /// <param name="assemblyName">The assembly name.</param>
        /// <returns>A string of the form <c>TypeName, AssemblyName</c>, where commas in <paramref name="typeName"/> have been properly escaped.</returns>
        public static string GetAssemblyQualifiedTypeName( string typeName, string assemblyName )
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

        /// <summary>
        /// Returns any method (static, instance, any accessibility) from the given type or its base types.
        /// </summary>
        public static MethodInfo? GetAnyMethod( this Type type, string name )
            => type.GetMethod( name, BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic )
               ?? type.BaseType?.GetAnyMethod( name );
    }
}