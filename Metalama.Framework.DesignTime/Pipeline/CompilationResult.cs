// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.Pipeline.Diff;
using Metalama.Framework.Engine.Pipeline;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.Pipeline;

internal sealed class CompilationResult
{
    public AspectPipelineResult AspectPipelineResult { get; }

    public AspectPipelineConfiguration AspectPipelineConfiguration { get; }

    public ProjectVersion ProjectVersion { get; }

    public DesignTimeAspectPipelineStatus AspectPipelineStatus { get; }

    internal CompilationResult(
        ProjectVersion projectVersion,
        AspectPipelineResult aspectPipelineResult,
        DesignTimeAspectPipelineStatus aspectPipelineStatus,
        AspectPipelineConfiguration aspectPipelineConfiguration )
    {
        this.AspectPipelineStatus = aspectPipelineStatus;
        this.AspectPipelineConfiguration = aspectPipelineConfiguration;
        this.AspectPipelineResult = aspectPipelineResult;
        this.ProjectVersion = projectVersion;
    }

    internal ImmutableArray<Diagnostic> GetAllDiagnostics( string path )
    {
        if ( this.AspectPipelineResult.SyntaxTreeResults.TryGetValue( path, out var syntaxTreeResults ) )
        {
            return syntaxTreeResults.Diagnostics;
        }
        else
        {
            return ImmutableArray<Diagnostic>.Empty;
        }
    }

    internal ImmutableArray<CacheableScopedSuppression> GetSuppressionOnSyntaxTree( string path )
    {
        if ( this.AspectPipelineResult.SyntaxTreeResults.TryGetValue( path, out var syntaxTreeResults ) )
        {
            return syntaxTreeResults.Suppressions;
        }
        else
        {
            return ImmutableArray<CacheableScopedSuppression>.Empty;
        }
    }

    internal (ImmutableArray<Diagnostic> Diagnostics, ImmutableArray<CacheableScopedSuppression> Suppressions) GetDiagnosticsOnSyntaxTree( string path )
    {
        var fromPipeline = this.AspectPipelineResult.GetDiagnosticsOnSyntaxTree( path );

        return (fromPipeline.Diagnostics, fromPipeline.Suppressions);
    }
}