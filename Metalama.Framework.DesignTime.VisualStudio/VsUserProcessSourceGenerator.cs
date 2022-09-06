// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.SourceGeneration;
using Metalama.Framework.Engine.Options;
using Metalama.Framework.Engine.Pipeline;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.DesignTime.VisualStudio;

public class VsUserProcessSourceGenerator : BaseSourceGenerator
{
    protected override ProjectHandler CreateSourceGeneratorImpl( IProjectOptions projectOptions )
        => new VsUserProcessProjectHandler( this.ServiceProvider, projectOptions );

    protected override void OnGeneratedSourceRequested( Compilation compilation, MSBuildProjectOptions options, CancellationToken cancellationToken )
    {
        // In the DevEnv process, we always serve from cache because the initiator of the source generator pipeline is always a change in the touch file
        // done by the analysis process, and this change is done after the devenv process receives the generated code from the named pipe.
    }

    // This constructor is called by the facade.
    public VsUserProcessSourceGenerator() : this( VsServiceProviderFactory.GetServiceProvider() ) { }

    internal VsUserProcessSourceGenerator( ServiceProvider serviceProvider ) : base( serviceProvider ) { }
}