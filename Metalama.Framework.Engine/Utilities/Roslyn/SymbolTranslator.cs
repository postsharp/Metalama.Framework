// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.Services;
using Microsoft.CodeAnalysis;
using System.Collections.Concurrent;

namespace Metalama.Framework.Engine.Utilities.Roslyn;

internal sealed partial class SymbolTranslator
{
    private readonly ConcurrentDictionary<ISymbol, ISymbol?> _cache;
    private readonly CompilationContext _targetCompilationContext;
    private readonly Visitor _visitor;

    internal SymbolTranslator( CompilationContext targetCompilationContextContext )
    {
        this._cache = new ConcurrentDictionary<ISymbol, ISymbol?>( ReferenceEqualityComparer<ISymbol>.Instance );

        this._targetCompilationContext = targetCompilationContextContext;
        this._visitor = new Visitor( this );
    }

    public T? Translate<T>( T symbol )
        where T : ISymbol
    {
        var containingAssembly = symbol.ContainingAssembly;

        if ( containingAssembly != null && this._targetCompilationContext.Assemblies.TryGetValue( containingAssembly.Identity, out var assembly )
                                        && assembly.Equals( containingAssembly ) )
        {
            // The symbol is guaranteed to be in the same assembly.
            return symbol;
        }
        else
        {
            return (T?) this._cache.GetOrAdd( symbol, this.TranslateCore );
        }
    }

    public T? Translate<T>( T symbol, Compilation? originalCompilation )
        where T : ISymbol
    {
        if ( originalCompilation == this._targetCompilationContext.Compilation )
        {
            return symbol;
        }
        else
        {
            return (T?) this._cache.GetOrAdd( symbol, this.TranslateCore );
        }
    }

    private ISymbol? TranslateCore( ISymbol symbol ) => this._visitor.Visit( symbol );
}