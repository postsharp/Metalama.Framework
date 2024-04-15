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

    internal ImmutableArray<CacheableScopedSuppression> GetSuppressionsOnSyntaxTree( string path )
    {
        if ( this.Result.SyntaxTreeResults.TryGetValue( path, out var syntaxTreeResult ) )
        {
            return syntaxTreeResult.Suppressions;
        }
        else
        {
            return ImmutableArray<CacheableScopedSuppression>.Empty;
        }
    }

    internal ImmutableArray<Diagnostic> GetDiagnosticsOnSyntaxTree( string path )
    {
        if ( this.Result.SyntaxTreeResults.TryGetValue( path, out var syntaxTreeResult ) )
        {
            return syntaxTreeResult.Diagnostics;
        }
        else
        {
            return ImmutableArray<Diagnostic>.Empty;
        }
    }
}