// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.Pipeline;
using Metalama.Framework.Engine;
using Metalama.Framework.Engine.Options;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Engine.Testing;
using Metalama.TestFramework;
using Microsoft.CodeAnalysis;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Metalama.Framework.Tests.UnitTests.DesignTime;

internal class TestDesignTimeAspectPipelineFactory : DesignTimeAspectPipelineFactory
{
    private readonly IProjectOptions _projectOptions;

    public TestDesignTimeAspectPipelineFactory( TestContext testContext, ServiceProvider? serviceProvider = null ) :
        base(
            serviceProvider ?? testContext.ServiceProvider,
            new UnloadableCompileTimeDomain(),
            true )
    {
        this._projectOptions = testContext.ProjectOptions;
    }

    protected override ValueTask<DesignTimeAspectPipeline?> GetPipelineAndWaitAsync( Compilation compilation, CancellationToken cancellationToken )
    {
        return new ValueTask<DesignTimeAspectPipeline?>( this.GetOrCreatePipeline( this._projectOptions, compilation, CancellationToken.None ) );
    }

    public override bool IsMetalamaEnabled( Compilation compilation )
        => compilation.References.OfType<PortableExecutableReference>()
            .Any( x => Path.GetFileNameWithoutExtension( x.FilePath )!.Equals( "Metalama.Framework", StringComparison.OrdinalIgnoreCase ) );

    public DesignTimeAspectPipeline CreatePipeline( Compilation compilation )
        => this.GetOrCreatePipeline( this._projectOptions, compilation, CancellationToken.None ).AssertNotNull();

    public override void Dispose()
    {
        base.Dispose();

        // Disposing the domain crashes the CLR in many design-time tests.
        //       this.Domain.Dispose();
    }
}