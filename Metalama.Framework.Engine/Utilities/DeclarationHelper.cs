﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Types;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Utilities.Caching;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Metalama.Framework.Engine.Utilities;

internal static class DeclarationHelper
{
    private enum TypeNameKind
    {
        Name,
        FullName
    }

    private static readonly WeakCache<IType, string> _reflectionNameCache = new();
    private static readonly WeakCache<IType, string> _reflectionFullNameCache = new();

    /// <summary>
    /// Gets a string that would be equal to <see cref="MemberInfo.Name"/>.
    /// </summary>
    internal static string GetReflectionName( this IType type )
        => _reflectionNameCache.GetOrAdd( type, x => x.GetReflectionName( TypeNameKind.Name ) );

    /// <summary>
    /// Gets a string that would be equal to <see cref="Type.FullName"/>, except that we do not qualify type names with the assembly name.
    /// </summary>
    internal static string GetReflectionFullName( this IType type )
        => _reflectionFullNameCache.GetOrAdd( type, x => x.GetReflectionName( TypeNameKind.FullName ) );

    private static string GetReflectionName( this IType declaration, TypeNameKind kind )
    {
        if ( declaration is ITypeParameter typeParameter )
        {
            return typeParameter.Name;
        }

        var sb = new StringBuilder();

        Append( declaration );

        return sb.ToString();

        void Append( ICompilationElement declaration, List<IType>? typeArguments = null )
        {
            var currentTypeArguments = typeArguments ?? [];

            // Append the containing namespace or type.
            if ( kind != TypeNameKind.Name && declaration is not ITypeParameter )
            {
                switch ( (declaration as IDeclaration)?.GetContainingDeclarationOrNamespace() )
                {
                    case null:
                        break;

                    case IType type:
                        Append( type, currentTypeArguments );

                        sb.Append( '+' );

                        break;

                    case INamespace ns:
                        if ( !ns.IsGlobalNamespace )
                        {
                            Append( ns );

                            sb.Append( '.' );
                        }

                        break;

                    default:
                        // A type is always contained in another type or in a namespace, possibly the global namespace.
                        throw new AssertionFailedException( $"'{declaration}' has an unexpected containing declaration {(declaration as IDeclaration)?.GetContainingDeclarationOrNamespace()}." );
                }
            }

            switch ( declaration )
            {
                case INamedType { IsGeneric: true } unboundGenericType
                    when !unboundGenericType.IsCanonicalGenericInstance && kind != TypeNameKind.Name:
                    sb.Append( unboundGenericType.GetMetadataName() );

                    currentTypeArguments.AddRange( unboundGenericType.TypeArguments );

                    break;

                case INamedDeclaration namedDeclaration:
                    sb.Append( namedDeclaration.GetMetadataName() );

                    break;

                case IArrayType array:
                    Append( array.ElementType );

                    sb.Append( '[' );

                    for ( var i = 1; i < array.Rank; i++ )
                    {
                        sb.Append( ',' );
                    }

                    sb.Append( ']' );

                    break;

                case IPointerType pointer:
                    Append( pointer.PointedAtType );

                    sb.Append( '*' );

                    break;

                case IDynamicType:
                    sb.Append( "System.Object" );

                    break;

                default:
                    throw new AssertionFailedException( $"Don't know how to process a {declaration?.GetType()}." );
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

    private static string GetMetadataName( this INamedDeclaration declaration )
        => declaration is IGeneric { TypeArguments.Count: > 0 and var count }
            ? $"{declaration.Name}`{count}"
            : declaration.Name;
}