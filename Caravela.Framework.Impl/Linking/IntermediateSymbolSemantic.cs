// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using System;

namespace Caravela.Framework.Impl.Linking
{
    internal struct IntermediateSymbolSemantic : IEquatable<IntermediateSymbolSemantic>
    {
        public ISymbol Symbol { get; }

        public IntermediateSymbolSemanticKind Kind { get; }

        public IntermediateSymbolSemantic( ISymbol symbol, IntermediateSymbolSemanticKind semantic )
        {
            this.Symbol = symbol;
            this.Kind = semantic;
        }

        public bool Equals( IntermediateSymbolSemantic other )
        {
            return SymbolEqualityComparer.Default.Equals( this.Symbol, other.Symbol )
                && other.Kind == this.Kind;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(
                SymbolEqualityComparer.Default.GetHashCode( this.Symbol ),
                this.Kind );
        }

        public IntermediateSymbolSemantic<TSymbol> ToTyped<TSymbol>()
            where TSymbol : ISymbol
        {
            return new IntermediateSymbolSemantic<TSymbol>( (TSymbol) this.Symbol, this.Kind );
        }

        public override string ToString()
        {
            return $"({this.Kind}, {this.Symbol})";
        }
    }

    internal struct IntermediateSymbolSemantic<TSymbol>
        where TSymbol : ISymbol
    {
        public TSymbol Symbol { get; }

        public IntermediateSymbolSemanticKind Kind { get; }

        public IntermediateSymbolSemantic( TSymbol symbol, IntermediateSymbolSemanticKind semantic )
        {
            this.Symbol = symbol;
            this.Kind = semantic;
        }

        public bool Equals( IntermediateSymbolSemantic other )
        {
            return SymbolEqualityComparer.Default.Equals( this.Symbol, other.Symbol )
                && other.Kind == this.Kind;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(
                SymbolEqualityComparer.Default.GetHashCode( this.Symbol ),
                this.Kind );
        }

        public static implicit operator IntermediateSymbolSemantic(IntermediateSymbolSemantic<TSymbol> value)
        {
            return new IntermediateSymbolSemantic( value.Symbol, value.Kind );
        }

        public override string ToString()
        {
            return $"({this.Kind}, {this.Symbol})";
        }
    }
}