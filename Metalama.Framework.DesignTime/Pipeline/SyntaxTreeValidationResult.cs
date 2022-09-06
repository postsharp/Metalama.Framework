// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.Pipeline;

internal sealed class SyntaxTreeValidationResult
{
    public SyntaxTree SyntaxTree { get; }

    public ImmutableArray<Diagnostic> Diagnostics { get; }

    public ImmutableArray<CacheableScopedSuppression> Suppressions { get; }

    public SyntaxTreeValidationResult( SyntaxTree syntaxTree, ImmutableArray<Diagnostic> diagnostics, ImmutableArray<CacheableScopedSuppression> suppressions )
    {
        this.SyntaxTree = syntaxTree;
        this.Diagnostics = diagnostics;
        this.Suppressions = suppressions;
    }
}