// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Project;

namespace Metalama.Framework.DesignTime.VisualStudio;

public class VsCodeFixProvider : TheCodeFixProvider
{
    public VsCodeFixProvider( ServiceProvider<IService> serviceProvider ) : base( serviceProvider ) { }

    public VsCodeFixProvider() : this( VsServiceProviderFactory.GetServiceProvider() ) { }
}