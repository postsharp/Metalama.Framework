// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.Pipeline.Diff;
using Metalama.Framework.Engine.CompileTime;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.Pipeline;

internal sealed class CompilationResult
{
    public CompilationValidationResult ValidationResult { get; }

    public CompilationPipelineResult TransformationResult { get; }

    public ProjectVersion ProjectVersion { get; }

    public CompileTimeProject? CompileTimeProject { get; }

    public DesignTimeAspectPipelineStatus PipelineStatus { get; }

    internal CompilationResult(
        in ProjectVersion projectVersion,
        CompilationPipelineResult transformationResult,
        CompilationValidationResult validationResult,
        CompileTimeProject? compileTimeProject,
        DesignTimeAspectPipelineStatus pipelineStatus )
    {
        this.ValidationResult = validationResult;
        this.CompileTimeProject = compileTimeProject;
        this.PipelineStatus = pipelineStatus;
        this.TransformationResult = transformationResult;
        this.ProjectVersion = projectVersion;
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