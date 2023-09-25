// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.Pipeline.Diff;
using Metalama.Framework.Engine.Pipeline;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.Pipeline;

internal sealed class AspectPipelineResultAndState
{
    public AspectPipelineResult Result { get; }

    public AspectPipelineConfiguration Configuration { get; }

    public ProjectVersion ProjectVersion { get; }

    public DesignTimeAspectPipelineStatus Status { get; }

    internal AspectPipelineResultAndState(
        ProjectVersion projectVersion,
        AspectPipelineResult result,
        DesignTimeAspectPipelineStatus status,
        AspectPipelineConfiguration configuration )
    {
        this.Status = status;
        this.Configuration = configuration;
        this.Result = result;
        this.ProjectVersion = projectVersion;
    }

    internal IEnumerable<Diagnostic> GetAllDiagnostics() => this.Result.SyntaxTreeResults.SelectMany( x => x.Value.Diagnostics );
    
    internal ImmutableArray<Diagnostic> GetAllDiagnostics( string path )
    {
        if ( this.Result.SyntaxTreeResults.TryGetValue( path, out var syntaxTreeResults ) )
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
        if ( this.Result.SyntaxTreeResults.TryGetValue( path, out var syntaxTreeResults ) )
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
        var fromPipeline = this.Result.GetDiagnosticsOnSyntaxTree( path );

        return (fromPipeline.Diagnostics, fromPipeline.Suppressions);
    }
}