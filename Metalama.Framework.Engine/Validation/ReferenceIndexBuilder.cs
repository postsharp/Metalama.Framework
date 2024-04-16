// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Services;
using Metalama.Framework.Validation;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Metalama.Framework.Engine.Validation;

public sealed class ReferenceIndexBuilder
{
    private readonly ConcurrentDictionary<ISymbol, ConcurrentDictionary<ISymbol, List<ReferencingNode>>> _references = new();
    private readonly ProjectServiceProvider _serviceProvider;
    private readonly IReferenceIndexerOptions _options;
    private bool _frozen;

    public ReferenceIndexBuilder( ProjectServiceProvider serviceProvider, IReferenceIndexerOptions options )
    {
        this._serviceProvider = serviceProvider;
        this._options = options;
    }

    [Conditional( "DEBUG" )]
    private static void CheckSymbolKind( ISymbol symbol )
    {
        switch ( symbol.Kind )
        {
            case SymbolKind.Local:
            case SymbolKind.Alias:
            case SymbolKind.Label:
            case SymbolKind.Preprocessing:
            case SymbolKind.DynamicType:
                throw new ArgumentOutOfRangeException();
        }

        if ( symbol is IMethodSymbol { MethodKind: MethodKind.LocalFunction } )
        {
            throw new ArgumentOutOfRangeException();
        }
    }

    internal void AddReference( ISymbol referencedSymbol, ISymbol referencingSymbol, SyntaxNodeOrToken node, ReferenceKinds referenceKinds )
    {
        CheckSymbolKind( referencedSymbol );
        CheckSymbolKind( referencingSymbol );

        var referencingSymbols = this._references.GetOrAddNew( referencedSymbol );
        var nodes = referencingSymbols.GetOrAddNew( referencingSymbol );

        lock ( nodes )
        {
            nodes.Add( new ReferencingNode( node, referenceKinds ) );
        }
    }

    public void Index( SemanticModel semanticModel, CancellationToken cancellationToken )
    {
        if ( this._frozen )
        {
            throw new InvalidOperationException();
        }

        var visitor = new ReferenceIndexWalker( this._serviceProvider, cancellationToken, this, this._options );
        visitor.Visit( semanticModel );
    }

    public ReferenceIndex ToReadOnly()
    {
        this._frozen = true;

        return new ReferenceIndex( this._references );
    }
}