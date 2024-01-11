﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Services;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.Linking
{
    internal sealed class AspectReferenceTargetEqualityComparer : IEqualityComparer<AspectReferenceTarget>
    {
        private readonly IEqualityComparer<ISymbol> _symbolComparer;

        internal AspectReferenceTargetEqualityComparer( IEqualityComparer<ISymbol> symbolComparer)
        {
            this._symbolComparer = symbolComparer;
        }
        
        public static IEqualityComparer<AspectReferenceTarget> ForCompilation(CompilationContext context)
        {
            return new AspectReferenceTargetEqualityComparer( context.SymbolComparer );
        }

        public bool Equals( AspectReferenceTarget x, AspectReferenceTarget y )
        {
            return this._symbolComparer.Equals( x.Symbol, y.Symbol )
                   && x.SemanticKind == y.SemanticKind
                   && x.TargetKind == y.TargetKind;
        }

        public int GetHashCode( AspectReferenceTarget x )
        {
            return HashCode.Combine(
                this._symbolComparer.GetHashCode( x.Symbol ),
                x.SemanticKind,
                x.TargetKind );
        }
    }
}