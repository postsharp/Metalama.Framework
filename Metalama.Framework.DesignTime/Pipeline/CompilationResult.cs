// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Aspects;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.Pipeline;

internal sealed class CompilationResult
{
    public CompilationValidationResult ValidationResult { get; }

    public CompilationPipelineResult TransformationResult { get; }

    public CompilationVersion CompilationVersion { get; }

    internal CompilationResult( CompilationVersion compilationVersion, CompilationPipelineResult transformationResult, CompilationValidationResult validationResult )
    {
        this.ValidationResult = validationResult;
        this.TransformationResult = transformationResult;
        this.CompilationVersion = compilationVersion;
    }

    internal IEnumerable<Diagnostic> GetAllDiagnostics( string path )
    {
        if ( this.TransformationResult.SyntaxTreeResults.TryGetValue( path, out var syntaxTreeResults ) )
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
        foreach ( var syntaxTree in this.TransformationResult.SyntaxTreeResults.Values )
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
        foreach ( var syntaxTree in this.TransformationResult.SyntaxTreeResults )
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
        if ( this.TransformationResult.SyntaxTreeResults.TryGetValue( path, out var syntaxTreeResults ) )
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
        var fromPipeline = this.TransformationResult.GetDiagnosticsOnSyntaxTree( path );
        var fromValidation = this.ValidationResult.GetDiagnosticsOnSyntaxTree( path );

        return (fromPipeline.Diagnostics.AddRange( fromValidation.Diagnostics ), fromPipeline.Suppressions.AddRange( fromValidation.Suppressions ));
    }
}