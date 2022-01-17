// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.DesignTime.Pipeline;

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