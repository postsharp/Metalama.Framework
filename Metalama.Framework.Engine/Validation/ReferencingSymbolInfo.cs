// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.Validation;

public readonly struct ReferencingSymbolInfo
{
    internal ReferencingSymbolInfo( ISymbol referencingSymbol, IReadOnlyList<ReferencingNode> nodes )
    {
        this.ReferencingSymbol = referencingSymbol;
        this.Nodes = nodes;
    }

    public ISymbol ReferencingSymbol { get; }

    public IReadOnlyList<ReferencingNode> Nodes { get; }
}