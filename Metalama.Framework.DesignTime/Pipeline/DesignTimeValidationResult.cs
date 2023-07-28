// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.Pipeline;

internal sealed class DesignTimeValidationResult
{
    public static DesignTimeValidationResult Empty { get; } = new();

    internal ImmutableDictionary<string, SyntaxTreeValidationResult> SyntaxTreeResults { get; }

    public DesignTimeValidatorCollectionEqualityKey ValidatorEqualityKey { get; }

    private DesignTimeValidationResult() : this( ImmutableDictionary<string, SyntaxTreeValidationResult>.Empty, default ) { }

    internal DesignTimeValidationResult(
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