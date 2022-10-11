// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Utilities.Caching;
using Microsoft.CodeAnalysis;
using System.Collections.Concurrent;

namespace Metalama.Framework.Engine.Utilities.Roslyn;

internal class SemanticModelProvider
{
    private readonly Compilation _compilation;
    private static readonly WeakCache<Compilation, SemanticModelProvider> _instances = new();
    private readonly ConcurrentDictionary<SyntaxTree, Cached> _semanticModels = new();

    private SemanticModelProvider( Compilation compilation )
    {
        this._compilation = compilation;
    }

    public static SemanticModelProvider GetInstance( Compilation compilation ) => _instances.GetOrAdd( compilation, c => new SemanticModelProvider( c ) );

    public SemanticModel GetSemanticModel( SyntaxTree syntaxTree, bool ignoreAccessibility )
    {
        var node = this._semanticModels.GetOrAdd( syntaxTree, tree => new Cached() );

        if ( ignoreAccessibility )
        {
            node.IgnoringAccessiblity ??= this._compilation.GetSemanticModel( syntaxTree, true );

            return node.IgnoringAccessiblity;
        }
        else
        {
            node.Default ??= this._compilation.GetSemanticModel( syntaxTree );

            return node.Default;
        }
    }

    private class Cached
    {
        public SemanticModel? Default { get; set; }

        public SemanticModel? IgnoringAccessiblity { get; set; }
    }
}