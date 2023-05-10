// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.CodeLens;
using Metalama.Framework.Engine.Pipeline.DesignTime;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Metalama.Framework.Project;
using Metalama.Framework.Tests.UnitTests.DesignTime.Mocks;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Framework.Tests.UnitTests.DesignTime;

#pragma warning disable VSTHRD200

public sealed class CodeLensTests : DesignTimeTestBase
{
    public CodeLensTests( ITestOutputHelper? testOutputHelper ) : base( testOutputHelper ) { }

    protected override void ConfigureServices( IAdditionalServiceCollection services )
    {
        base.ConfigureServices( services );
        services.AddGlobalService( provider => new TestWorkspaceProvider( provider ) );
    }

    [Fact]
    public async Task AspectAddingAspect()
    {
        using var testContext = this.CreateTestContext();
        using TestDesignTimeAspectPipelineFactory factory = new( testContext );

        const string code = """
            using Metalama.Framework.Aspects;
            using Metalama.Framework.Code;
            using System;
            using System.IO;

            public class InjectedLoggerAttribute : OverrideMethodAspect
            {
                [Introduce]
                private readonly TextWriter _logger = Console.Out;

                public override dynamic? OverrideMethod()
                {
                    _logger.WriteLine("Logged.");
                    return meta.Proceed();
                }
            }
            
            public class RepositoryAspect : TypeAspect
            {
                public override void BuildAspect(IAspectBuilder<INamedType> builder)
                {
                    builder.Advice.IntroduceMethod(builder.Target, nameof(Get));

                    builder.Outbound.SelectMany(type => type.Methods)
                        .AddAspectIfEligible<InjectedLoggerAttribute>();
                }

                [Template]
                public object Get(int id) => id.ToString();            
            }

            [RepositoryAspect]
            public class Repository
            {
            }
            """;

        var workspaceProvider = factory.ServiceProvider.GetRequiredService<TestWorkspaceProvider>();
        var projectKey = workspaceProvider.AddOrUpdateProject( "project", new Dictionary<string, string> { ["code.cs"] = code } );

        var compilation = (await workspaceProvider.GetCompilationAsync( projectKey ))!;

        // We need to run the pipeline because code lens does not run it on its own.
        var pipeline = factory.CreatePipeline( compilation );
        await pipeline.ExecuteAsync( compilation, AsyncExecutionContext.Get() );

        // Test the CodeLens service.
        var targetClassId = compilation.GetTypeByMetadataName( "Repository" )!.GetSerializableId();

        var codeLensService = new CodeLensServiceImpl( factory.ServiceProvider );

        var summary = await codeLensService.GetCodeLensSummaryAsync( projectKey, targetClassId, default );

        Assert.Equal( "2 aspects", summary.Description );

        var details = await codeLensService.GetCodeLensDetailsAsync( projectKey, targetClassId, default );

        Assert.Equal(
            new[]
            {
                new[] { "RepositoryAspect", "Repository", "Custom attribute", "Introduce method 'Repository.Get(int)'." },
                new[] { "InjectedLogger", "Repository.Get(int)", "Child of 'Repository'", "Introduce field 'Repository._logger'." }
            },
            details.Entries.SelectAsEnumerable( e => e.Fields.SelectAsEnumerable( f => f.Text ) ) );
    }
}