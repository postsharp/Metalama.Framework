// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.DesignTime.SourceGeneration;
using Metalama.Framework.Engine.Options;
using Metalama.Framework.Engine.Pipeline;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Metalama.Framework.DesignTime;

/// <summary>
/// An implementation of <see cref="BaseSourceGenerator"/> that should execute in the analysis process.
/// </summary>
public class AnalysisProcessSourceGenerator : BaseSourceGenerator
{
    protected override ProjectHandler CreateSourceGeneratorImpl( IProjectOptions projectOptions )
        => new AnalysisProcessProjectHandler( this.ServiceProvider, projectOptions );


    protected override void OnGeneratedSourceRequested(
        Compilation compilation,
        MSBuildProjectOptions options,
        CancellationToken cancellationToken )
    {
        // If there is a cached compilation result, this will schedule a background computation of the compilation even if the TouchId is unchanged.
        // If there is no cached result, this will perform a synchronous computation and the next call will return it from cache.
        
        _ = this.GetGeneratedSources( compilation, options, cancellationToken );
    }

    // This constructor is called by the facade.
    public AnalysisProcessSourceGenerator() : this( DesignTimeServiceProviderFactory.GetServiceProvider() ) { }

    public AnalysisProcessSourceGenerator( ServiceProvider serviceProvider ) : base( serviceProvider ) { }
}