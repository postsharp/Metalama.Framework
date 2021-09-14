// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.ReflectionMocks;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Caravela.Framework.Impl.CompileTime
{
    /// <summary>
    /// Provides the <see cref="GetCompileTimeType"/> method, which maps a Roslyn <see cref="ITypeSymbol"/> to a reflection <see cref="Type"/>.
    /// </summary>
    internal abstract class CompileTimeTypeResolver
    {
        /// <summary>
        /// Maps a Roslyn <see cref="!:ITypeSymbol" /> to a reflection <see cref="!:Type" />. 
        /// </summary>
        protected abstract Type? GetCompileTimeNamedType( INamedTypeSymbol typeSymbol, CancellationToken cancellationToken = default );

        public Type? GetCompileTimeType( ITypeSymbol typeSymbol, bool fallbackToMock, CancellationToken cancellationToken = default )
        {
            Type? NullOrMock() => fallbackToMock ? CompileTimeType.Create( typeSymbol ) : null;

            switch ( typeSymbol )
            {
                case IArrayTypeSymbol arrayType:
                    {
                        var elementType = this.GetCompileTimeType( arrayType.ElementType, false, cancellationToken );

                        if ( elementType == null )
                        {
                            return NullOrMock();
                        }

                        if ( arrayType.IsSZArray )
                        {
                            return elementType.MakeArrayType();
                        }
                        else
                        {
                            return elementType.MakeArrayType( arrayType.Rank );
                        }
                    }

                case INamedTypeSymbol { IsGenericType: true } genericType when !genericType.IsGenericTypeDefinition():
                    {
                        var typeDefinition = this.GetCompileTimeNamedType( genericType.ConstructedFrom );

                        if ( typeDefinition == null )
                        {
                            return NullOrMock();
                        }

                        var typeArguments = CollectTypeArguments( genericType )
                            .Select( arg => this.GetCompileTimeType( arg, false, cancellationToken ) )
                            .ToArray();

                        if ( typeArguments.Contains( null ) )
                        {
                            return NullOrMock();
                        }

                        return typeDefinition.MakeGenericType( typeArguments );
                    }

                case INamedTypeSymbol namedType:
                    return this.GetCompileTimeNamedType( namedType, cancellationToken ) ?? NullOrMock();

                case IDynamicTypeSymbol:
                    // This cannot happen because the method is called only for types constructed with typeof, and typeof(dynamic) is forbidden.
                    throw new AssertionFailedException();

                case IPointerTypeSymbol pointerType:
                    {
                        var elementType = this.GetCompileTimeType( pointerType.PointedAtType, fallbackToMock, cancellationToken );

                        if ( elementType == null )
                        {
                            return NullOrMock();
                        }
                        else
                        {
                            return elementType.MakePointerType();
                        }
                    }

                case ITypeParameterSymbol:
                    {
                        // It is not possible to get this symbol using a typeof expression on a custom attribute.
                        throw new AssertionFailedException();
                    }

                default:
                    throw new AssertionFailedException( $"Don't know how to map the '{typeSymbol}' type." );
            }

            static IEnumerable<ITypeSymbol> CollectTypeArguments( INamedTypeSymbol? s )
            {
                var typeArguments = new List<ITypeSymbol>();

                while ( s != null )
                {
                    typeArguments.InsertRange( 0, s.TypeArguments );

                    s = s.ContainingSymbol as INamedTypeSymbol;
                }

                return typeArguments;
            }
        }
    }
}