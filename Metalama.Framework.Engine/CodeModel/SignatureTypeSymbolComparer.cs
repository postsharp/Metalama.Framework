// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Utilities.Comparers;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel
{
    /// <summary>
    /// Compares symbols for purposes of method signature comparison.
    /// </summary>
    internal sealed class SignatureTypeSymbolComparer : IEqualityComparer<ISymbol?>
    {
        public static SignatureTypeSymbolComparer Instance { get; } = new( StructuralSymbolComparer.Default );

        private readonly StructuralSymbolComparer _inner;

        private SignatureTypeSymbolComparer( StructuralSymbolComparer inner )
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

                case (IParameterSymbol leftParameter, IParameterSymbol rightParameter):
                    return
                        this.Equals( leftParameter.Type, rightParameter.Type )
                        && leftParameter.RefKind == rightParameter.RefKind;

                case (IMethodSymbol leftMethod, IMethodSymbol rightMethod):
                    // Whole method signature matching.
                    return
                        StringComparer.Ordinal.Equals( leftMethod.Name, rightMethod.Name )
                        && leftMethod.TypeParameters.Length == rightMethod.TypeParameters.Length
                        && this.Equals( leftMethod.ReturnType, rightMethod.ReturnType )
                        && leftMethod.Parameters.SequenceEqual( rightMethod.Parameters, ( l, r ) => this.Equals( l, r ) );

                case (IEventSymbol leftEvent, IEventSymbol rightEvent):
                    return
                        StringComparer.Ordinal.Equals( leftEvent.Name, rightEvent.Name )
                        && this.Equals( leftEvent.Type, rightEvent.Type );

                case (IPropertySymbol leftProperty, IPropertySymbol rightProperty):
                    return
                        StringComparer.Ordinal.Equals( leftProperty.Name, rightProperty.Name )
                        && this.Equals( leftProperty.Type, rightProperty.Type )
                        && leftProperty.Parameters.SequenceEqual( rightProperty.Parameters, ( l, r ) => this.Equals( l, r ) );

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
            switch ( obj )
            {
                case null:
                    return 0;

                case IArrayTypeSymbol array:
                    return HashCode.Combine( this.GetHashCode( array ) );

                case IDynamicTypeSymbol:
                    return HashCode.Combine( 0x57446317 );

                case ITypeParameterSymbol typeParameter:
                    return HashCode.Combine( typeParameter.TypeParameterKind, typeParameter.Ordinal );

                case IPointerTypeSymbol pointerType:
                    return HashCode.Combine( this.GetHashCode( pointerType.PointedAtType ) );

                case IFunctionPointerTypeSymbol functionPointerType:
                    return
                        HashCode.Combine( this.GetHashCode( functionPointerType.Signature ) );

                case IParameterSymbol parameterSymbol:
                    return HashCode.Combine( parameterSymbol.Ordinal );

                case IMethodSymbol method:
                    // Whole method signature matching.
                    return
                        HashCode.Combine(
                            method.Name,
                            method.TypeParameters.Length,
                            method.Parameters.Length );

                case INamedTypeSymbol namedType:
                    return
                        HashCode.Combine( namedType.MetadataName );

                default:
                    return this._inner.GetHashCode( obj );
            }
        }
    }
}