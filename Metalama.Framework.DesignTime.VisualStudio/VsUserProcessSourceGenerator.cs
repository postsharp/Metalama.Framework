// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Metalama.Framework.DesignTime.Rpc;
using Metalama.Framework.DesignTime.SourceGeneration;
using Metalama.Framework.DesignTime.VisualStudio.Remoting.UserProcess;
using Metalama.Framework.DesignTime.VisualStudio.Services;
using Metalama.Framework.Engine.Options;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities.Threading;
using Metalama.Framework.Project;
using Metalama.Framework.Services;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.DesignTime.VisualStudio;

[UsedImplicitly]
public class VsUserProcessSourceGenerator : BaseSourceGenerator
{
    protected override ProjectHandler CreateSourceGeneratorImpl( IProjectOptions projectOptions, ProjectKey projectKey )
        => new VsUserProcessProjectHandler( this.ServiceProvider, projectOptions, projectKey );

    protected override void OnGeneratedSourceRequested( Compilation compilation, IProjectOptions options, TestableCancellationToken cancellationToken )
    {
        // In the DevEnv process, we always serve from cache because the initiator of the source generator pipeline is always a change in the touch file
        // done by the analysis process, and this change is done after the devenv process receives the generated code from the named pipe.
    }

    // This constructor is called by the facade.
    public VsUserProcessSourceGenerator() : this( VsServiceProviderFactory.GetServiceProvider() ) { }

    internal VsUserProcessSourceGenerator( ServiceProvider<IGlobalService> serviceProvider ) : base( serviceProvider )
    {
#if DEBUG
        _ = serviceProvider.GetRequiredService<UserProcessServiceHubEndpoint>();
#endif
    }
}