// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Options;
using Metalama.Framework.Engine.Pipeline;

namespace Metalama.Framework.DesignTime.VisualStudio;

public class VsUserProcessSourceGenerator : TheSourceGenerator
{
    protected override ProjectHandler CreateSourceGeneratorImpl( IProjectOptions projectOptions )
        => new VsUserProcessProjectHandler( this.ServiceProvider, projectOptions );

    // This constructor is called by the facade.
    public VsUserProcessSourceGenerator() : this( VsServiceProviderFactory.GetServiceProvider() ) { }

    internal VsUserProcessSourceGenerator( ServiceProvider serviceProvider ) : base( serviceProvider ) { }
}