// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.Validation;

internal readonly struct ReferencingSymbolInfo
{
    internal ReferencingSymbolInfo( ISymbol referencingSymbol, ReferencingNodeList nodes )
    {
        this.ReferencingSymbol = referencingSymbol;
        this.Nodes = nodes;
    }

    public ISymbol ReferencingSymbol { get; }

    public ReferencingNodeList Nodes { get; }
}