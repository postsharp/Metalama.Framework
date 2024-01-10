﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Services;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;

#pragma warning disable SA1402 // File may only contain a single type

namespace Metalama.Framework.Engine.Linking
{
    internal sealed class IntermediateSymbolSemanticEqualityComparer : IEqualityComparer<IntermediateSymbolSemantic>
    {
        private readonly IEqualityComparer<ISymbol> _symbolComparer;

        internal IntermediateSymbolSemanticEqualityComparer(IEqualityComparer<ISymbol> symbolComparer)
        {
            this._symbolComparer = symbolComparer;
        }
        
        public static IEqualityComparer<IntermediateSymbolSemantic> ForCompilation(CompilationContext context)
        {
            return new IntermediateSymbolSemanticEqualityComparer( context.SymbolComparer );
        }

        public bool Equals( IntermediateSymbolSemantic x, IntermediateSymbolSemantic y )
        {
            return this._symbolComparer.Equals( x.Symbol, y.Symbol )
                   && x.Kind == y.Kind;
        }

        public int GetHashCode( IntermediateSymbolSemantic x )
        {
            return HashCode.Combine(
                this._symbolComparer.GetHashCode( x.Symbol ),
                x.Kind );
        }
    }
    internal sealed class IntermediateSymbolSemanticEqualityComparer<TSymbol> : IEqualityComparer<IntermediateSymbolSemantic<TSymbol>>
        where TSymbol : ISymbol
    {
        private readonly IEqualityComparer<ISymbol> _symbolComparer;

        internal IntermediateSymbolSemanticEqualityComparer( IEqualityComparer<ISymbol> symbolComparer )
        {
            this._symbolComparer = symbolComparer;
        }

        public static IEqualityComparer<IntermediateSymbolSemantic<TSymbol>> ForCompilation( CompilationContext context )
        {
            return new IntermediateSymbolSemanticEqualityComparer<TSymbol>( context.SymbolComparer );
        }

        public bool Equals( IntermediateSymbolSemantic<TSymbol> x, IntermediateSymbolSemantic<TSymbol> y )
        {
            return this._symbolComparer.Equals( x.Symbol, y.Symbol )
                   && x.Kind == y.Kind;
        }

        public int GetHashCode( IntermediateSymbolSemantic<TSymbol> x )
        {
            // PERF: Cast enum to int otherwise it will be boxed within the method.
            return HashCode.Combine(
                this._symbolComparer.GetHashCode( x.Symbol ),
                x.Kind );
        }
    }
}