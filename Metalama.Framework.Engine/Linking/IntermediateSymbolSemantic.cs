﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Utilities.Comparers;
using Microsoft.CodeAnalysis;
using System;

namespace Metalama.Framework.Engine.Linking
{
    internal readonly struct IntermediateSymbolSemantic : IEquatable<IntermediateSymbolSemantic>
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
            return StructuralSymbolComparer.Default.Equals( this.Symbol, other.Symbol )
                   && other.Kind == this.Kind;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(
                StructuralSymbolComparer.Default.GetHashCode( this.Symbol ),
                this.Kind );
        }

        public IntermediateSymbolSemantic<TSymbol> ToTyped<TSymbol>()
            where TSymbol : ISymbol
        {
            return new IntermediateSymbolSemantic<TSymbol>( (TSymbol) this.Symbol, this.Kind );
        }

        public override string ToString()
        {
            // Coverage: ignore (useful for debugging)
            return $"({this.Kind}, {this.Symbol})";
        }
    }

    internal readonly struct IntermediateSymbolSemantic<TSymbol> : IEquatable<IntermediateSymbolSemantic<TSymbol>>
        where TSymbol : ISymbol
    {
        public TSymbol Symbol { get; }

        public IntermediateSymbolSemanticKind Kind { get; }

        public IntermediateSymbolSemantic( TSymbol symbol, IntermediateSymbolSemanticKind semantic )
        {
            this.Symbol = symbol;
            this.Kind = semantic;
        }

        public bool Equals( IntermediateSymbolSemantic<TSymbol> other )
        {
            // No typed dictionary at the moment.
            throw new AssertionFailedException( Justifications.CoverageMissing );

            // return SymbolEqualityComparer.Default.Equals( this.Symbol, other.Symbol )
            //       && other.Kind == this.Kind;
        }

        public override int GetHashCode()
        {
            // No typed dictionary at the moment.
            throw new AssertionFailedException( Justifications.CoverageMissing );

            // return HashCode.Combine(
            //    SymbolEqualityComparer.Default.GetHashCode( this.Symbol ),
            //    this.Kind );
        }

        public static implicit operator IntermediateSymbolSemantic( IntermediateSymbolSemantic<TSymbol> value )
        {
            return new IntermediateSymbolSemantic( value.Symbol, value.Kind );
        }

        public override string ToString()
        {
            // Coverage: ignore (useful for debugging)
            return $"({this.Kind}, {this.Symbol})";
        }
    }
}