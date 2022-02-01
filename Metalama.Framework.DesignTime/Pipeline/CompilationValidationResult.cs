// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.Pipeline;

internal class CompilationValidationResult
{
    public static CompilationValidationResult Empty { get; } = new();

    internal ImmutableDictionary<string, SyntaxTreeValidationResult> SyntaxTreeResults { get; }

    public DesignTimeValidatorCollectionEqualityKey ValidatorEqualityKey { get; }

    private CompilationValidationResult() : this( ImmutableDictionary<string, SyntaxTreeValidationResult>.Empty, default ) { }

    internal CompilationValidationResult(
        ImmutableDictionary<string, SyntaxTreeValidationResult> syntaxTreeResults,
        DesignTimeValidatorCollectionEqualityKey validatorEqualityKey )
    {
        this.SyntaxTreeResults = syntaxTreeResults;
        this.ValidatorEqualityKey = validatorEqualityKey;
    }

    internal (ImmutableArray<Diagnostic> Diagnostics, ImmutableArray<CacheableScopedSuppression> Suppressions) GetDiagnosticsOnSyntaxTree( string path )
    {
        if ( this.SyntaxTreeResults.TryGetValue( path, out var syntaxTreeResult ) )
        {
            return (syntaxTreeResult.Diagnostics, syntaxTreeResult.Suppressions);
        }
        else
        {
            return (ImmutableArray<Diagnostic>.Empty, ImmutableArray<CacheableScopedSuppression>.Empty);
        }
    }
}