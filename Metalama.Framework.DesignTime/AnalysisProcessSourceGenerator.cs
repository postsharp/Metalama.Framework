// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.Rpc;
using Metalama.Framework.DesignTime.SourceGeneration;
using Metalama.Framework.Engine.Options;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Engine.Utilities.Threading;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.DesignTime;

/// <summary>
/// An implementation of <see cref="BaseSourceGenerator"/> that should execute in the analysis process.
/// </summary>
public class AnalysisProcessSourceGenerator : BaseSourceGenerator
{
    protected override ProjectHandler CreateSourceGeneratorImpl( IProjectOptions projectOptions, ProjectKey projectKey )
        => new AnalysisProcessProjectHandler( this.ServiceProvider, projectOptions, projectKey );

    protected override void OnGeneratedSourceRequested(
        Compilation compilation,
        MSBuildProjectOptions options,
        TestableCancellationToken cancellationToken )
    {
        // If there is a cached compilation result, this will schedule a background computation of the compilation even if the TouchId is unchanged.
        // If there is no cached result, this will perform a synchronous computation and the next call will return it from cache.

        _ = this.GetGeneratedSources( compilation, options, cancellationToken );
    }

    // This constructor is called by the facade.
    public AnalysisProcessSourceGenerator() : this( DesignTimeServiceProviderFactory.GetServiceProvider( false ) ) { }

    public AnalysisProcessSourceGenerator( ServiceProvider serviceProvider ) : base( serviceProvider ) { }
}