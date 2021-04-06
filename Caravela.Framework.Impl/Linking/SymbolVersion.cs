﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.Linking
{
    /// <summary>
    /// Represents a version of the symbol, i.e. a pair of ISymbol and AspectLayerId?.
    /// </summary>
    internal struct SymbolVersion : IEquatable<SymbolVersion>
    {
        public ISymbol Symbol { get; }

        public AspectLayerId? AspectLayer { get; }

        public SymbolVersion(ISymbol symbol, AspectLayerId? aspectLayer)
        {
            this.Symbol = symbol;
            this.AspectLayer = aspectLayer;
        }

        public bool Equals( SymbolVersion other )
        {
            return
                this.AspectLayer == other.AspectLayer
                && StructuralSymbolComparer.Instance.Equals( this.Symbol, other.Symbol );
        }

        public override bool Equals( object obj )
        {
            return obj is SymbolVersion && this.Equals( (SymbolVersion) obj );
        }

        public override int GetHashCode()
        {
            return StructuralSymbolComparer.Instance.GetHashCode(this.Symbol) ^ (this.AspectLayer?.GetHashCode() ?? 0);
        }

        public static bool operator ==( SymbolVersion left, SymbolVersion right )
        {
            return left.Equals( right );
        }

        public static bool operator !=( SymbolVersion left, SymbolVersion right )
        {
            return !(left == right);
        }
    }
}
