// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Compiler;
using Metalama.Framework.Engine.Options;
using Metalama.Framework.Engine.Pipeline;

namespace Metalama.Framework.DesignTime.VisualStudio;

public class VsAnalysisProcessSourceGenerator : AnalysisProcessSourceGenerator
{
#pragma warning disable CA1001 // ServiceHost is disposable but not owned.

    static VsAnalysisProcessSourceGenerator()
    {
        if ( !MetalamaCompilerInfo.IsActive )
        {
            CompilerServiceProvider.Initialize();
        }
    }

    public VsAnalysisProcessSourceGenerator() : this( VisualStudioServiceProviderFactory.GetServiceProvider() ) { }

    public VsAnalysisProcessSourceGenerator( ServiceProvider serviceProvider ) : base( serviceProvider ) { }

    protected override ProjectHandler CreateSourceGeneratorImpl( IProjectOptions projectOptions )
        => new VsAnalysisProcessProjectHandler( this.ServiceProvider, projectOptions );
}