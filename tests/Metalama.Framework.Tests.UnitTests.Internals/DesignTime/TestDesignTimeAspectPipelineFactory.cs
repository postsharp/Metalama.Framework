// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.Pipeline;
using Metalama.Framework.Engine;
using Metalama.Framework.Engine.Options;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Engine.Testing;
using Metalama.Framework.Project;
using Metalama.TestFramework;
using Microsoft.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Metalama.Framework.Tests.UnitTests.DesignTime;

internal class TestDesignTimeAspectPipelineFactory : DesignTimeAspectPipelineFactory
{
    private readonly IProjectOptions _projectOptions;
    private static readonly TestMetalamaProjectClassifier _projectClassifier = new();

    private static ServiceProvider GetServiceProvider( TestContext testContext, ServiceProvider? serviceProvider = null )
    {
        serviceProvider ??= testContext.ServiceProvider;
        if ( serviceProvider.GetService<AnalysisProcessEventHub>() == null )
        {
            serviceProvider = serviceProvider.WithService( new AnalysisProcessEventHub( serviceProvider ) );
        }

        return serviceProvider;
    }


    public TestDesignTimeAspectPipelineFactory( TestContext testContext, ServiceProvider? serviceProvider = null ) :
        base(
            GetServiceProvider( testContext, serviceProvider ),
            new UnloadableCompileTimeDomain(),
            true )
    {
        this._projectOptions = testContext.ProjectOptions;
    }

    protected override ValueTask<DesignTimeAspectPipeline?> GetPipelineAndWaitAsync( Compilation compilation, CancellationToken cancellationToken )
    {
        return new ValueTask<DesignTimeAspectPipeline?>( this.GetOrCreatePipeline( this._projectOptions, compilation ) );
    }

    public override bool IsMetalamaEnabled( Compilation compilation ) => _projectClassifier.IsMetalamaEnabled( compilation );

    public DesignTimeAspectPipeline CreatePipeline( Compilation compilation ) => this.GetOrCreatePipeline( this._projectOptions, compilation ).AssertNotNull();
}