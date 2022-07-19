// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel
{
    /// <summary>
    /// Compares symbols for purposes of method signature comparison.
    /// </summary>
    internal class SignatureTypeSymbolComparer : IEqualityComparer<ISymbol?>
    {
        public static SignatureTypeSymbolComparer Instance { get; } = new( SymbolEqualityComparer.Default );

        private readonly SymbolEqualityComparer _inner;

        private SignatureTypeSymbolComparer( SymbolEqualityComparer inner )
        {
            this._inner = inner;
        }

        public bool Equals( ISymbol? left, ISymbol? right )
        {
            switch (left, right)
            {
                case (IArrayTypeSymbol leftArray, IArrayTypeSymbol rightArray):
                    return leftArray.Rank == rightArray.Rank
                           && this.Equals( leftArray.ElementType, rightArray.ElementType );

                case (IDynamicTypeSymbol, IDynamicTypeSymbol):
                    return true;

                case (ITypeParameterSymbol leftTypeParameter, ITypeParameterSymbol rightTypeParameter):
                    // TODO: Interpret compile-time parameters for generic interfaces.
                    return
                        leftTypeParameter.TypeParameterKind == rightTypeParameter.TypeParameterKind
                        && leftTypeParameter.Ordinal == rightTypeParameter.Ordinal;

                case (IPointerTypeSymbol leftPointerType, IPointerTypeSymbol rightPointerType):
                    // TODO: Constraints.
                    return this.Equals( leftPointerType.PointedAtType, rightPointerType.PointedAtType );

                case (IFunctionPointerTypeSymbol leftFunctionPointerType, IFunctionPointerTypeSymbol rightFunctionPointerType):
                    return
                        this.Equals( leftFunctionPointerType.Signature, rightFunctionPointerType.Signature );

                case (IMethodSymbol leftMethod, IMethodSymbol rightMethod):
                    // Whole method signature matching.
                    return
                        StringComparer.Ordinal.Equals( leftMethod.Name, rightMethod.Name )
                        && leftMethod.TypeParameters.Length == rightMethod.TypeParameters.Length
                        && this.Equals( leftMethod.ReturnType, rightMethod.ReturnType )
                        && leftMethod.Parameters.SequenceEqual(
                            rightMethod.Parameters,
                            ( l, r ) =>
                                l.RefKind == r.RefKind
                                && this.Equals( l, r ) );

                case (INamedTypeSymbol leftNamedType, INamedTypeSymbol rightNamedType):
                    return
                        this._inner.Equals( leftNamedType.OriginalDefinition, rightNamedType.OriginalDefinition )
                        && leftNamedType.TypeArguments.SequenceEqual(
                            rightNamedType.TypeArguments,
                            ( l, r ) => this.Equals( l, r ) );

                default:
                    return this._inner.Equals( left, right );
            }
        }

        public int GetHashCode( ISymbol? obj )
        {
            throw new NotImplementedException();
        }
    }
}