// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.Validation;

internal readonly struct ReferencingSymbolInfo
{
    internal ReferencingSymbolInfo( ISymbol referencingSymbol, ISymbol referencedSymbol, ReferencingNodeList nodes )
    {
        this.ReferencingSymbol = referencingSymbol;
        this.ReferencedSymbol = referencedSymbol;
        this.Nodes = nodes;
    }

    public ISymbol ReferencingSymbol { get; }

    public ISymbol ReferencedSymbol { get; }

    public ReferencingNodeList Nodes { get; }
}