// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.Pipeline;
using Metalama.Framework.DesignTime.Rpc.Notifications;
using Metalama.Framework.DesignTime.VisualStudio.Remoting.AnalysisProcess;
using Metalama.Framework.DesignTime.VisualStudio.Remoting.UserProcess;
using Metalama.Framework.Engine;
using Metalama.Testing.UnitTesting;
using Metalama.Testing.UnitTesting.Options;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Framework.Tests.UnitTests.DesignTime;

#pragma warning disable VSTHRD200

public class NotificationIntegrationTests : UnitTestClass
{
    public NotificationIntegrationTests( ITestOutputHelper logger ) : base( logger ) { }

    [Fact]
    public async Task ReceivesNotification()
    {
        using var testContext = this.CreateTestContext( new TestContextOptions() { HasSourceGeneratorTouchFile = true } );
        var serviceProvider = testContext.ServiceProvider.Global;
        serviceProvider = serviceProvider.WithService( new AnalysisProcessEventHub( serviceProvider ) );

        // Start the hub service on both ends.
        var testGuid = Guid.NewGuid();
        var hubPipeName = $"Metalama_Hub_{testGuid}";
        var servicePipeName = $"Metalama_Analysis_{testGuid}";

        using var userProcessServiceHubEndpoint = new UserProcessServiceHubEndpoint( serviceProvider, hubPipeName );
        userProcessServiceHubEndpoint.Start();
        using var analysisProcessServiceHubEndpoint = new AnalysisProcessServiceHubEndpoint( serviceProvider, hubPipeName );
        _ = analysisProcessServiceHubEndpoint.ConnectAsync(); // Do not await so we get more randomness.

        // Start the main services on both ends.
        using var analysisProcessEndpoint = new AnalysisProcessEndpoint(
            serviceProvider.WithService( analysisProcessServiceHubEndpoint ),
            servicePipeName );

        analysisProcessEndpoint.Start();

        // Start the notification listener.
        var notificationListenerEndpoint = new NotificationListenerEndpoint( serviceProvider.Underlying, hubPipeName );
        _ = notificationListenerEndpoint.ConnectAsync();

        BlockingCollection<CompilationResultChangedEventArgs> eventQueue = new();

        notificationListenerEndpoint.CompilationResultChanged += args => eventQueue.Add( args );

        var pipelineFactory = new TestDesignTimeAspectPipelineFactory( testContext, serviceProvider.WithService( analysisProcessServiceHubEndpoint ) );

        var compilation1 = TestCompilationFactory.CreateCSharpCompilation( "", name: "project" );
        var pipeline = pipelineFactory.GetOrCreatePipeline( testContext.ProjectOptions, compilation1 ).AssertNotNull();

        // The first pipeline execution should notify a full compilation.
        await pipeline.ExecuteAsync( compilation1 );
        var notification1 = eventQueue.Take( testContext.CancellationToken );

        Assert.False( notification1.IsPartialCompilation );

        var compilation2 = TestCompilationFactory.CreateCSharpCompilation( "class C{}", name: "project" );
        await pipeline.ExecuteAsync( compilation2 );
        var notification2 = eventQueue.Take( testContext.CancellationToken );

        Assert.True( notification2.IsPartialCompilation );
    }
}