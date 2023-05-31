// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Metalama.Framework.DesignTime.Rpc;
using Metalama.Framework.DesignTime.VisualStudio.Services;
using Metalama.Framework.Engine.Options;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Project;
using Metalama.Framework.Services;

namespace Metalama.Framework.DesignTime.VisualStudio;

[UsedImplicitly]
public class VsAnalysisProcessSourceGenerator : AnalysisProcessSourceGenerator
{
    private readonly VsAnalysisProcessProjectHandlerFactory _projectHandlerFactory;

    public VsAnalysisProcessSourceGenerator() : this( VsServiceProviderFactory.GetServiceProvider() ) { }

    public VsAnalysisProcessSourceGenerator( ServiceProvider<IGlobalService> serviceProvider ) : base( serviceProvider )
    {
        this._projectHandlerFactory = serviceProvider.GetRequiredService<VsAnalysisProcessProjectHandlerFactory>();
    }

    protected override ProjectHandler CreateSourceGeneratorImpl( IProjectOptions projectOptions, ProjectKey projectKey )
        => this._projectHandlerFactory.GetOrCreateProjectHandler( projectOptions, projectKey );
}