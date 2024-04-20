﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Validation;
using Microsoft.CodeAnalysis;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Tests.UnitTests.Validation;

internal class ReferenceIndexObserver : IReferenceIndexObserver
{
    private readonly ConcurrentQueue<ISymbol> _resolvedSymbols = new();
    private readonly ConcurrentQueue<SemanticModel> _resolvedSemanticModels = new();

    public IReadOnlyCollection<ISymbol> ResolvedSymbols => this._resolvedSymbols;

    public IReadOnlyList<string> ResolvedSymbolNames
        => this._resolvedSymbols.SelectAsReadOnlyCollection( s => s.ToTestName() ).OrderBy( x => x ).Distinct().ToReadOnlyList();

    public IReadOnlyCollection<SemanticModel> ResolvedSemanticModels => this._resolvedSemanticModels;

    public IReadOnlyCollection<string> ResolvedSemanticModelNames
        => this._resolvedSemanticModels.SelectAsReadOnlyCollection( m => m.SyntaxTree.FilePath ).OrderBy( x => x ).ToReadOnlyList();

    public void OnSymbolResolved( ISymbol? symbol )
    {
        if ( symbol != null )
        {
            this._resolvedSymbols.Enqueue( symbol );
        }
    }

    public void OnSemanticModelResolved( SemanticModel semanticModel )
    {
        this._resolvedSemanticModels.Enqueue( semanticModel );
    }
}