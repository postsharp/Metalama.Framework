// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.Utilities.Comparers
{

    internal class StructuralSymbolComparer<T> : StructuralSymbolComparer, IEqualityComparer<T>
        where T : ISymbol
    {
        public static new readonly StructuralSymbolComparer<T> Default =
            new(
                StructuralSymbolComparerOptions.ContainingDeclaration |
                StructuralSymbolComparerOptions.Name |
                StructuralSymbolComparerOptions.GenericParameterCount |
                StructuralSymbolComparerOptions.ParameterTypes |
                StructuralSymbolComparerOptions.ParameterModifiers );

        public static new readonly StructuralSymbolComparer<T> Signature =
            new(
                StructuralSymbolComparerOptions.Name |
                StructuralSymbolComparerOptions.GenericParameterCount |
                StructuralSymbolComparerOptions.ParameterTypes |
                StructuralSymbolComparerOptions.ParameterModifiers );

        public StructuralSymbolComparer( StructuralSymbolComparerOptions options ) : base( options )
        {
        }

        public bool Equals( T x, T y )
        {
            return base.Equals( x, y );
        }

        public int GetHashCode( T obj )
        {
            return base.GetHashCode( obj );
        }
    }
}