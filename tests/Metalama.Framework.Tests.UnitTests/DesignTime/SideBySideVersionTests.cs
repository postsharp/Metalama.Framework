// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.Contracts.EntryPoint;
using Metalama.Framework.DesignTime.VersionNeutral;
using Metalama.Framework.Engine.Pipeline.DesignTime;
using Metalama.Framework.Tests.UnitTests.DesignTime.Mocks;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.DesignTime;

#pragma warning disable VSTHRD200

public sealed class SideBySideVersionTests : DesignTimeTestBase
{
    [Fact]
    public async Task Test()
    {
        using var testContext = this.CreateTestContext();
        var workspaceProvider = new TestWorkspaceProvider( testContext.ServiceProvider );

        var entryPointManager = new DesignTimeEntryPointManager();
        var consumer = entryPointManager.GetConsumer( CurrentContractVersions.All );

        var serviceProvider = testContext.ServiceProvider.Global.Underlying.WithUntypedService( typeof(IDesignTimeEntryPointConsumer), consumer )
            .WithService( workspaceProvider );

        // Initialize dependencies simulating the current version.
        var currentVersionPipelineFactory = new TestDesignTimeAspectPipelineFactory( testContext, serviceProvider );
        var currentVersionServiceProvider = new CompilerServiceProvider( version: TestMetalamaProjectClassifier.CurrentMetalamaVersion );
        currentVersionServiceProvider.Initialize( serviceProvider.WithService( currentVersionPipelineFactory ) );
        entryPointManager.RegisterServiceProvider( currentVersionServiceProvider );

        // Initialize dependencies simulating the _other_ version.
        var otherVersionPipelineFactory = new TestDesignTimeAspectPipelineFactory( testContext, serviceProvider );
        var otherVersionServiceProvider = new CompilerServiceProvider( version: TestMetalamaProjectClassifier.OtherMetalamaVersion );
        otherVersionServiceProvider.Initialize( serviceProvider.WithService( otherVersionPipelineFactory ) );
        entryPointManager.RegisterServiceProvider( otherVersionServiceProvider );

        const string masterCode = """
                                  using System;
                                  using Metalama.Framework.Aspects;

                                  [Inheritable]
                                  public class TheAspect : TypeAspect
                                  {
                                   [Introduce( WhenExists = OverrideStrategy.New )]
                                    public void IntroducedMethod() {}
                                  }

                                  [TheAspect]
                                  public interface TheInterface
                                  {

                                  }

                                  """;

        const string dependentCode = """
                                     public class TheClass : TheInterface
                                     {
                                        
                                     }

                                     """;

        workspaceProvider.AddOrUpdateProject(
            "master",
            new Dictionary<string, string>() { ["master.cs"] = masterCode },
            preprocessorSymbols: new[] { "METALAMA", TestMetalamaProjectClassifier.OtherMetalamaVersionPreprocessorSymbol } );

        var dependentProjectKey = workspaceProvider.AddOrUpdateProject(
            "dependent",
            new Dictionary<string, string>() { ["dependent.cs"] = dependentCode },
            projectReferences: new[] { "master" } );

        var dependentCompilation = await workspaceProvider.GetCompilationAsync( dependentProjectKey );

        var result = await currentVersionPipelineFactory.ExecuteAsync( dependentCompilation!, AsyncExecutionContext.Get() );

        Assert.Single( result.Value.Result.SyntaxTreeResults.Single().Value.AspectInstances );
    }
}