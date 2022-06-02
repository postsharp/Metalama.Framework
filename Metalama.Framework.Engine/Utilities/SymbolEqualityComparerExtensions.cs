// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Metalama.Framework.Engine.Utilities
{
    internal static class SymbolEqualityComparerExtensions
    {
        /// <summary>
        /// Compares equality of two parameter types, specifically interpreting and comparing ITypeParameterSymbol's ordinals.
        /// </summary>
        public static bool ParameterTypeEquals(this SymbolEqualityComparer comparer, ISymbol? left, ISymbol? right)
        {
            if ( left is ITypeParameterSymbol leftTypeParam && right is ITypeParameterSymbol rightTypeParam )
            {
                return leftTypeParam.Ordinal == rightTypeParam.Ordinal;
            }
            else
            {
                return comparer.Equals( left, right );
            }
        }
    }
}
