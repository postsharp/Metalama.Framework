﻿using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Caravela.Framework.Impl.Linking
{
    internal class StructuralSymbolComparer : IEqualityComparer<ISymbol>
    {
        public static readonly StructuralSymbolComparer Instance = new StructuralSymbolComparer();

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
