// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Utilities.Caching;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace Metalama.Framework.Engine.Utilities.Roslyn;

public static class CompilationExtensions
{
    private static readonly ConditionalWeakTable<Compilation, ImmutableDictionary<string, SyntaxTree>> _indexedSyntaxTreesCache = new();

    public static ImmutableDictionary<string, SyntaxTree> GetIndexedSyntaxTrees( this Compilation compilation )
        => _indexedSyntaxTreesCache.GetOrAdd( compilation, GetIndexedSyntaxTreesCore );

    private static ImmutableDictionary<string, SyntaxTree> GetIndexedSyntaxTreesCore( Compilation compilation )
        => compilation.SyntaxTrees.ToImmutableDictionary( x => x.FilePath, x => x );
}