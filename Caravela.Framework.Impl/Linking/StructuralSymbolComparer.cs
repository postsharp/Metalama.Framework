// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.Linking
{
    /// <summary>
    /// Compares symbols, possibly from different compilations.
    /// </summary>
    internal class StructuralSymbolComparer : IEqualityComparer<ISymbol>
    {
        // TODO: At this point the default display string seems to be enough for comparison.

        public static readonly StructuralSymbolComparer Instance = new();

        public bool Equals( ISymbol x, ISymbol y )
        {
            return x.ToDisplayString() == y.ToDisplayString();
        }

        public int GetHashCode( ISymbol obj )
        {
            return obj.ToDisplayString().GetHashCode();
        }
    }
}