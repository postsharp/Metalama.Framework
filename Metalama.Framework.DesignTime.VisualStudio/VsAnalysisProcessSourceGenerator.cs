// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Options;
using Metalama.Framework.Engine.Pipeline;

namespace Metalama.Framework.DesignTime.VisualStudio;

public class VsAnalysisProcessSourceGenerator : AnalysisProcessSourceGenerator
{
    public VsAnalysisProcessSourceGenerator() : this( VsServiceProviderFactory.GetServiceProvider() ) { }

    public VsAnalysisProcessSourceGenerator( ServiceProvider serviceProvider ) : base( serviceProvider ) { }

    protected override ProjectHandler CreateSourceGeneratorImpl( IProjectOptions projectOptions, ProjectKey projectKey )
        => new VsAnalysisProcessProjectHandler( this.ServiceProvider, projectOptions, projectKey );
}