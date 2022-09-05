// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.Utilities.Comparers
{

    internal class StructuralSymbolComparer<T> : StructuralSymbolComparer, IEqualityComparer<T>
        where T : ISymbol
    {
        public static new readonly StructuralSymbolComparer Default =
            new(
                StructuralSymbolComparerOptions.ContainingDeclaration |
                StructuralSymbolComparerOptions.Name |
                StructuralSymbolComparerOptions.GenericParameterCount |
                StructuralSymbolComparerOptions.ParameterTypes |
                StructuralSymbolComparerOptions.ParameterModifiers );

        public static new readonly StructuralSymbolComparer Signature =
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