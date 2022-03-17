// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Pipeline;

namespace Metalama.Framework.DesignTime.VisualStudio;

public class VsCodeFixProvider : TheCodeFixProvider
{
    public VsCodeFixProvider( ServiceProvider serviceProvider ) : base( serviceProvider ) { }

    public VsCodeFixProvider() : this( VsServiceProviderFactory.GetServiceProvider() ) { }
}