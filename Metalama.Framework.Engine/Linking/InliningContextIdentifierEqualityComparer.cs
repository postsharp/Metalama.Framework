// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Services;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;

// ReSharper disable NotAccessedPositionalProperty.Global

namespace Metalama.Framework.Engine.Linking
{
    internal sealed class InliningContextIdentifierEqualityComparer : IEqualityComparer<InliningContextIdentifier>
    {
        private readonly IEqualityComparer<ISymbol> _symbolComparer;

        internal InliningContextIdentifierEqualityComparer( IEqualityComparer<ISymbol> symbolComparer )
        {
            this._symbolComparer = symbolComparer;
        }

        public static IEqualityComparer<InliningContextIdentifier> ForCompilation( CompilationContext context )
        {
            return new InliningContextIdentifierEqualityComparer( context.SymbolComparer );
        }

        public bool Equals( InliningContextIdentifier? x, InliningContextIdentifier? y )
        {
            return
                ( x == null && y == null )
                || (
                    x != null && y != null &&
                    x.InliningId == y.InliningId
                    && this._symbolComparer.Equals( x.DestinationSemantic.Symbol, y.DestinationSemantic.Symbol )
                    && x.DestinationSemantic.Kind == y.DestinationSemantic.Kind );
        }

        public int GetHashCode( InliningContextIdentifier x )
        {
            return HashCode.Combine(
                x.InliningId,
                this._symbolComparer.GetHashCode( x.DestinationSemantic.Symbol ),
                x.DestinationSemantic.Kind );
        }
    }
}