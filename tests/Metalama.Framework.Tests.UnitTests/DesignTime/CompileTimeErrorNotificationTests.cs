// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.VisualStudio;
using Metalama.Framework.Engine.Pipeline.DesignTime;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Framework.Tests.UnitTests.DesignTime;

#pragma warning disable VSTHRD200

public sealed class CompileTimeErrorNotificationTests : DistributedDesignTimeTestBase
{
    public CompileTimeErrorNotificationTests( ITestOutputHelper? logger = null ) : base( logger ) { }

    [Fact]
    public async Task EndToEnd()
    {
        var analysisProcessServices = new ServiceProviderBuilder<IGlobalService>();

        // Initialize the components.
        using var testContext = this.CreateDistributedDesignTimeTestContext( null, analysisProcessServices, null );

        await testContext.WhenFullyInitialized;

        const string codeWithError = """
                                                 using Metalama.Framework.Aspects;
                                                 using Metalama.Framework.Code;
                                                 using System;
                                     
                                                 public class InjectedLoggerAttribute : OverrideMethodAspect
                                                 {
                                     
                                                     public override dynamic? OverrideMethod()
                                                     {
                                                         some error here
                                                     }
                                                 }

                                     """;

        // Initialize the workspace. We are initializing it with an error to check that we get the initial state correctly.
        var projectKey = testContext.WorkspaceProvider.AddOrUpdateProject( "project", new Dictionary<string, string> { ["code.cs"] = codeWithError } );
        await testContext.AnalysisProcessEndpoint.RegisterProjectAsync( projectKey );

        // Register a synchronization point.
        var hasCompilerErrorData = new TaskCompletionSource<bool>();
        testContext.UserProcessServiceHubEndpoint.Endpoints.Single().CompileTimeErrorsChanged += _ => hasCompilerErrorData.SetResult( true );

        // Execute the pipeline to get the errors.
        var project = testContext.WorkspaceProvider.GetProject( "project" );
        var pipeline = testContext.PipelineFactory.GetOrCreatePipeline( project )!;
        var result = await pipeline.ExecuteAsync( (await project.GetCompilationAsync())!, AsyncExecutionContext.Get() );
        Assert.False( result.IsSuccessful );
        Assert.NotEmpty( result.Diagnostics );

        // Make sure we create CompileTimeEditingStatusService after the pipeline has first executed.
        await hasCompilerErrorData.Task;

        // Instantiate the service.
        var errorService = new CompileTimeEditingStatusService( testContext.UserProcessServiceProvider );

        // Check that we have errors.
        Assert.NotEmpty( errorService.CompileTimeErrors );

        // Fix the error, run the pipeline, and check that the error collection is cleared.
        var hasCompilerErrorData2 = new TaskCompletionSource<bool>();
        errorService.CompileTimeErrorsChanged += () => hasCompilerErrorData2.SetResult( true );

        testContext.WorkspaceProvider.AddOrUpdateProject( "project", new Dictionary<string, string> { ["code.cs"] = "" } );

        var result2 = await pipeline.ExecuteAsync(
            (await testContext.WorkspaceProvider.GetProject( "project" ).GetCompilationAsync())!,
            AsyncExecutionContext.Get() );

        Assert.True( result2.IsSuccessful );
        Assert.Empty( result2.Diagnostics );

        await hasCompilerErrorData2.Task;
        Assert.Empty( errorService.CompileTimeErrors );
    }
}