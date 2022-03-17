// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Options;
using Metalama.Framework.Engine.Pipeline;

namespace Metalama.Framework.DesignTime;

/// <summary>
/// An implementation of <see cref="BaseSourceGenerator"/> that should execute in the analysis process.
/// </summary>
public class AnalysisProcessSourceGenerator : BaseSourceGenerator
{
    protected override ProjectHandler CreateSourceGeneratorImpl( IProjectOptions projectOptions )
        => new AnalysisProcessProjectHandler( this.ServiceProvider, projectOptions );

    // This constructor is called by the facade.
    public AnalysisProcessSourceGenerator() : this( DesignTimeServiceProviderFactory.GetServiceProvider() ) { }

    public AnalysisProcessSourceGenerator( ServiceProvider serviceProvider ) : base( serviceProvider ) { }
}