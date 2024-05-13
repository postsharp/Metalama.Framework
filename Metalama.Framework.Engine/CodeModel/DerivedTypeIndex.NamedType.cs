// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.CodeModel;

public sealed partial class DerivedTypeIndex
{
    /// <summary>
    /// Represents either a <see cref="INamedTypeSymbol"/> or <see cref="INamedType"/>.
    /// </summary>
    private readonly struct NamedType
    {
        private readonly object _value;

        public NamedType( INamedTypeSymbol symbol )
        {
            this._value = symbol;
        }

        public NamedType( INamedType type )
        {
            this._value = (object?) type.GetSymbol() ?? type;
        }

        public INamedTypeSymbol? Symbol => this._value as INamedTypeSymbol;

        // ReSharper disable once InconsistentNaming
        public INamedType? IType => this._value as INamedType;

        public INamedType ToIType( CompilationModel compilationModel )
            => this.IType?.Translate( compilationModel ) ?? compilationModel.Factory.GetNamedType( this.Symbol! );

        public override string? ToString() => this._value.ToString();

        public sealed class Comparer( IEqualityComparer<INamedTypeSymbol> symbolComparer, IEqualityComparer<INamedType> typeComparer )
            : IEqualityComparer<NamedType>
        {
            public bool Equals( NamedType x, NamedType y )
            {
                if ( x.Symbol != null && y.Symbol != null )
                {
                    return symbolComparer.Equals( x.Symbol, y.Symbol );
                }
                else if ( x.IType != null && y.IType != null )
                {
                    return typeComparer.Equals( x.IType, y.IType );
                }
                else
                {
                    return false;
                }
            }

            public int GetHashCode( NamedType obj )
            {
                if ( obj.Symbol != null )
                {
                    return symbolComparer.GetHashCode( obj.Symbol );
                }
                else
                {
                    return typeComparer.GetHashCode( obj.IType! );
                }
            }
        }
    }
}