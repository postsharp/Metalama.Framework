// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Aspects;
using Microsoft.CodeAnalysis;
using System;

namespace Caravela.Framework.Impl.Linking
{
    internal readonly struct AspectReferenceTarget : IEquatable<AspectReferenceTarget>
    {
        public ISymbol Symbol { get; }

        public IntermediateSymbolSemanticKind Semantic { get; }

        public AspectReferenceTargetKind TargetKind { get; }

        public AspectReferenceTarget( ISymbol symbol, IntermediateSymbolSemanticKind semantic, AspectReferenceTargetKind targetKind )
        {
            this.Symbol = symbol;
            this.Semantic = semantic;
            this.TargetKind = targetKind;
        }

        public bool Equals( AspectReferenceTarget other )
        {
            return SymbolEqualityComparer.Default.Equals( this.Symbol, other.Symbol )
                   && other.Semantic == this.Semantic
                   && other.TargetKind == this.TargetKind;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(
                SymbolEqualityComparer.Default.GetHashCode( this.Symbol ),
                this.Semantic,
                this.TargetKind );
        }
    }
}