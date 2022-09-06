// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.Pipeline;

internal sealed class CompilationResult
{
    public CompilationValidationResult ValidationResult { get; }

    public CompilationPipelineResult PipelineResult { get; }

    internal CompilationResult( CompilationPipelineResult pipelineResult, CompilationValidationResult validationResult )
    {
        this.ValidationResult = validationResult;
        this.PipelineResult = pipelineResult;
    }

    internal IEnumerable<Diagnostic> GetAllDiagnostics( string path )
    {
        if ( this.PipelineResult.SyntaxTreeResults.TryGetValue( path, out var syntaxTreeResults ) )
        {
            foreach ( var diagnostic in syntaxTreeResults.Diagnostics )
            {
                yield return diagnostic;
            }
        }

        if ( this.ValidationResult.SyntaxTreeResults.TryGetValue( path, out var validationResult ) )
        {
            foreach ( var diagnostic in validationResult.Diagnostics )
            {
                yield return diagnostic;
            }
        }
    }

    internal IEnumerable<Diagnostic> GetAllDiagnostics()
    {
        foreach ( var syntaxTree in this.PipelineResult.SyntaxTreeResults.Values )
        {
            foreach ( var diagnostic in syntaxTree.Diagnostics )
            {
                yield return diagnostic;
            }
        }

        foreach ( var syntaxTree in this.ValidationResult.SyntaxTreeResults )
        {
            foreach ( var diagnostic in syntaxTree.Value.Diagnostics )
            {
                yield return diagnostic;
            }
        }
    }

    internal IEnumerable<CacheableScopedSuppression> GetAllSuppressions()
    {
        foreach ( var syntaxTree in this.PipelineResult.SyntaxTreeResults )
        {
            foreach ( var diagnostic in syntaxTree.Value.Suppressions )
            {
                yield return diagnostic;
            }
        }

        foreach ( var syntaxTree in this.ValidationResult.SyntaxTreeResults )
        {
            foreach ( var diagnostic in syntaxTree.Value.Suppressions )
            {
                yield return diagnostic;
            }
        }
    }

    internal IEnumerable<CacheableScopedSuppression> GetAllSuppressions( string path )
    {
        if ( this.PipelineResult.SyntaxTreeResults.TryGetValue( path, out var syntaxTreeResults ) )
        {
            foreach ( var diagnostic in syntaxTreeResults.Suppressions )
            {
                yield return diagnostic;
            }
        }

        if ( this.ValidationResult.SyntaxTreeResults.TryGetValue( path, out var validationResult ) )
        {
            foreach ( var diagnostic in validationResult.Suppressions )
            {
                yield return diagnostic;
            }
        }
    }

    internal (ImmutableArray<Diagnostic> Diagnostics, ImmutableArray<CacheableScopedSuppression> Suppressions) GetDiagnosticsOnSyntaxTree( string path )
    {
        var fromPipeline = this.PipelineResult.GetDiagnosticsOnSyntaxTree( path );
        var fromValidation = this.ValidationResult.GetDiagnosticsOnSyntaxTree( path );

        return (fromPipeline.Diagnostics.AddRange( fromValidation.Diagnostics ), fromPipeline.Suppressions.AddRange( fromValidation.Suppressions ));
    }
}