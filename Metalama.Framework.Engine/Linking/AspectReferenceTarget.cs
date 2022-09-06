// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Utilities.Comparers;
using Microsoft.CodeAnalysis;
using System;

namespace Metalama.Framework.Engine.Linking
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
            return StructuralSymbolComparer.Default.Equals( this.Symbol, other.Symbol )
                   && other.Semantic == this.Semantic
                   && other.TargetKind == this.TargetKind;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(
                SignatureTypeSymbolComparer.Instance.GetHashCode( this.Symbol ),
                this.Semantic,
                this.TargetKind );
        }
    }
}