// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.Contracts.EntryPoint;
using Metalama.Framework.DesignTime.Services;
using Metalama.Framework.DesignTime.VersionNeutral;
using Metalama.Framework.DesignTime.VisualStudio.Remoting.UserProcess;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Services;

namespace Metalama.Framework.DesignTime.VisualStudio.Services;

internal sealed class VsUserProcessServiceProviderFactory : DesignTimeUserProcessServiceProviderFactory
{
    public VsUserProcessServiceProviderFactory() : this( null ) { }

    public VsUserProcessServiceProviderFactory( DesignTimeEntryPointManager? entryPointManager ) : base( entryPointManager ) { }

    protected override CompilerServiceProvider CreateCompilerServiceProvider() => new VsUserProcessCompilerServiceProvider();

    protected override ServiceProvider<IGlobalService>
        AddServices( ServiceProvider<IGlobalService> serviceProvider )
    {
        serviceProvider = base.AddServices( serviceProvider );

        var userProcessRegistrationService = UserProcessServiceHubEndpoint.GetInstance( serviceProvider );

        serviceProvider = serviceProvider.WithService( userProcessRegistrationService );
        serviceProvider = serviceProvider.WithService( new LocalWorkspaceProvider( serviceProvider ) );
        serviceProvider = serviceProvider.WithService( sp => new VsUserProcessInvalidationService( sp ) );

        return serviceProvider;
    }
}