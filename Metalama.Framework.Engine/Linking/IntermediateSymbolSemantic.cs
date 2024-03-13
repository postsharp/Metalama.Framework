// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Utilities.Comparers;
using Microsoft.CodeAnalysis;
using System;

namespace Metalama.Framework.Engine.Linking;

internal readonly struct IntermediateSymbolSemantic : IEquatable<IntermediateSymbolSemantic>
{
    public ISymbol Symbol { get; }

    public IntermediateSymbolSemanticKind Kind { get; }

    public IntermediateSymbolSemantic( ISymbol symbol, IntermediateSymbolSemanticKind semantic )
    {
        this.Symbol = symbol.GetCanonicalDefinition();
        this.Kind = semantic;
    }

    public bool Equals( IntermediateSymbolSemantic other )
        => StructuralSymbolComparer.Default.Equals( this.Symbol.OriginalDefinition, other.Symbol.OriginalDefinition )
           && other.Kind == this.Kind;

    public override int GetHashCode()
        =>

            // PERF: Cast enum to int otherwise it will be boxed on .NET Framework.
            HashCode.Combine(
                StructuralSymbolComparer.Default.GetHashCode( this.Symbol ),
                (int) this.Kind );

    public IntermediateSymbolSemantic<TSymbol> ToTyped<TSymbol>() 
        where TSymbol : ISymbol => new( (TSymbol) this.Symbol, this.Kind );

    public IntermediateSymbolSemantic WithSymbol( ISymbol symbol ) => new( symbol, this.Kind );

    public IntermediateSymbolSemantic<TSymbol> WithSymbol<TSymbol>( TSymbol symbol ) 
        where TSymbol : ISymbol => new( symbol, this.Kind );

    public override string ToString()
        =>

            // Coverage: ignore (useful for debugging)
            $"({this.Kind}, {this.Symbol})";
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
        => StructuralSymbolComparer.Default.Equals( this.Symbol.OriginalDefinition, other.Symbol.OriginalDefinition )
           && other.Kind == this.Kind;

    public override int GetHashCode()
        =>

            // PERF: Cast enum to byte otherwise it will be boxed on .NET Framework.
            HashCode.Combine(
                StructuralSymbolComparer.Default.GetHashCode( this.Symbol ),
                (byte) this.Kind );

    public static implicit operator IntermediateSymbolSemantic( IntermediateSymbolSemantic<TSymbol> value ) => new( value.Symbol, value.Kind );

    public override string ToString()
        =>

            // Coverage: ignore (useful for debugging)
            $"({this.Kind}, {this.Symbol})";
}