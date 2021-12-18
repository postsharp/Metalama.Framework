// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.DesignTime.Pipeline;

public sealed class CompilationResult
{
    public CompilationValidationResult ValidationResult { get; }

    public CompilationPipelineResult PipelineResult { get; }

    internal CompilationResult( CompilationPipelineResult pipelineResult, CompilationValidationResult validationResult )
    {
        this.ValidationResult = validationResult;
        this.PipelineResult = pipelineResult;
    }

    internal (ImmutableArray<Diagnostic> Diagnostics, ImmutableArray<CacheableScopedSuppression> Suppressions) GetDiagnosticsOnSyntaxTree( string path )
    {
        var fromPipeline = this.PipelineResult.GetDiagnosticsOnSyntaxTree( path );
        var fromValidation = this.ValidationResult.GetDiagnosticsOnSyntaxTree( path );

        return (fromPipeline.Diagnostics.AddRange( fromValidation.Diagnostics ), fromPipeline.Suppressions.AddRange( fromValidation.Suppressions ));
    }
}