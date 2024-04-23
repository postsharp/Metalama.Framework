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

    protected override ServiceProvider<IGlobalService> AddServices( ServiceProvider<IGlobalService> serviceProvider )
        => base.AddServices( serviceProvider )
            .WithService( sp => UserProcessServiceHubEndpoint.GetInstance( sp ) )
            .WithService( sp => new LocalWorkspaceProvider( sp ) );
}