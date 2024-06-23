// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Microsoft.CodeAnalysis;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.Validation;

internal sealed class ReferenceIndex
{
    private readonly ConcurrentDictionary<ISymbol, ReferencedSymbolInfo> _explicitReferences;

    internal ReferenceIndex( ConcurrentDictionary<ISymbol, ReferencedSymbolInfo> explicitReferences )
    {
        this._explicitReferences = explicitReferences;
    }

    public IEnumerable<ReferencedSymbolInfo> ReferencedSymbols => this._explicitReferences.Values;
}