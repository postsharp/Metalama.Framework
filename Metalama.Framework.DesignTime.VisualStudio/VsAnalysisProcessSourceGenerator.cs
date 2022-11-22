// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.Rpc;
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