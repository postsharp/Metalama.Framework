// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using System;

namespace Caravela.Framework.Impl.Linking
{
    /// <summary>
    /// Represents a version of the symbol, i.e. a pair of ISymbol and AspectLayerId?.
    /// </summary>
    internal readonly struct SymbolVersion : IEquatable<SymbolVersion>
    {
        public ISymbol Symbol { get; }

        public LinkerAnnotationTargetKind TargetKind { get; }

        public AspectLayerId? AspectLayer { get; }

        public SymbolVersion( ISymbol symbol, AspectLayerId? aspectLayer, LinkerAnnotationTargetKind targetKind )
        {
            this.Symbol = symbol;
            this.AspectLayer = aspectLayer;
            this.TargetKind = targetKind;
        }

        public bool Equals( SymbolVersion other )
        {
            return
                this.AspectLayer == other.AspectLayer
                && StructuralSymbolComparer.Default.Equals( this.Symbol, other.Symbol )
                && this.TargetKind == other.TargetKind;
        }

        public override bool Equals( object obj )
        {
            return obj is SymbolVersion version && this.Equals( version );
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(
                StructuralSymbolComparer.Default.GetHashCode( this.Symbol ),
                this.AspectLayer,
                this.TargetKind );
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