// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Compiler;
using Metalama.Framework.Engine.Utilities.Caching;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.Options;

// ReSharper disable once InconsistentNaming
public sealed class MSBuildProjectOptionsFactory : IDisposable, IProjectOptionsFactory
{
    private readonly TimeBasedCache<AnalyzerConfigOptions, MSBuildProjectOptions> _cache;

    public static MSBuildProjectOptionsFactory Default { get; } = new( MSBuildPropertyNames.All );

    public MSBuildProjectOptionsFactory( IEnumerable<string> relevantProperties )
    {
        this._cache = new TimeBasedCache<AnalyzerConfigOptions, MSBuildProjectOptions>(
            TimeSpan.FromMinutes( 10 ),
            new AnalyzerConfigOptionsComparer( relevantProperties ) );
    }

    public MSBuildProjectOptions GetProjectOptions(
        AnalyzerConfigOptionsProvider options,
        ImmutableArray<object>? plugIns = null,
        TransformerOptions? transformerOptions = null )
        => this.GetProjectOptions( options.GlobalOptions, plugIns, transformerOptions );

    private MSBuildProjectOptions GetProjectOptions(
        AnalyzerConfigOptions options,
        ImmutableArray<object>? plugIns = null,
        TransformerOptions? transformerOptions = null )
    {
        if ( plugIns != null || transformerOptions != null )
        {
            // We have a source transformer. Caching is useless.
            return new MSBuildProjectOptions( options, plugIns, transformerOptions );
        }
        else
        {
            // At design time, we should try to cache.
            return this._cache.GetOrAdd( options, o => new MSBuildProjectOptions( o ) );
        }
    }

    public MSBuildProjectOptions GetProjectOptions(
        Microsoft.CodeAnalysis.Project project,
        ImmutableArray<object>? plugIns = null,
        TransformerOptions? transformerOptions = null )
        => this.GetProjectOptions( project.AnalyzerOptions.AnalyzerConfigOptionsProvider.GlobalOptions, plugIns, transformerOptions );

    public void Dispose() => this._cache.Dispose();

    IProjectOptions IProjectOptionsFactory.GetProjectOptions( Microsoft.CodeAnalysis.Project project )
        => this.GetProjectOptions( project.AnalyzerOptions.AnalyzerConfigOptionsProvider );
}