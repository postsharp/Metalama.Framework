// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Utilities.Caching;
using Microsoft.CodeAnalysis;
using System.Collections.Concurrent;

namespace Metalama.Framework.Engine.Utilities.Roslyn;

public sealed class SemanticModelProvider
{
    private static readonly WeakCache<Compilation, SemanticModelProvider> _instances = new();
    private readonly Compilation _compilation;
    private readonly ConcurrentDictionary<SyntaxTree, Cached> _semanticModels = new();

    private SemanticModelProvider( Compilation compilation )
    {
        this._compilation = compilation;
    }

    internal static SemanticModelProvider GetInstance( Compilation compilation ) => _instances.GetOrAdd( compilation, c => new SemanticModelProvider( c ) );

    public static ISemanticModel GetSemanticModel( SemanticModel semanticModel )
    {
        var provider = GetInstance( semanticModel.Compilation );

        var node = provider._semanticModels.GetOrAdd( semanticModel.SyntaxTree, _ => new Cached() );

        if ( semanticModel.IgnoresAccessibility )
        {
            node.IgnoringAccessibility ??= new CachingSemanticModel( semanticModel );

            return node.IgnoringAccessibility;
        }
        else
        {
            node.Default ??= new CachingSemanticModel( semanticModel );

            return node.Default;
        }
    }

    public ISemanticModel GetSemanticModel( SyntaxTree syntaxTree, bool ignoreAccessibility = false )
    {
        var node = this._semanticModels.GetOrAdd( syntaxTree, _ => new Cached() );

        if ( ignoreAccessibility )
        {
            node.IgnoringAccessibility ??= new CachingSemanticModel( this._compilation.GetSemanticModel( syntaxTree, true ) );

            return node.IgnoringAccessibility;
        }
        else
        {
            node.Default ??= new CachingSemanticModel( this._compilation.GetSemanticModel( syntaxTree ) );

            return node.Default;
        }
    }

    private sealed class Cached
    {
        public ISemanticModel? Default { get; set; }

        public ISemanticModel? IgnoringAccessibility { get; set; }
    }
}