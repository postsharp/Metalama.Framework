// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.Pipeline;
using Metalama.Framework.DesignTime.Rpc.Notifications;
using Metalama.Framework.DesignTime.VisualStudio.Remoting.AnalysisProcess;
using Metalama.Framework.DesignTime.VisualStudio.Remoting.UserProcess;
using Metalama.Framework.Engine;
using Metalama.Framework.Engine.Testing;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Framework.Tests.UnitTests.DesignTime;

public class NotificationIntegrationTests : LoggingTestBase
{
    public NotificationIntegrationTests( ITestOutputHelper logger ) : base( logger ) { }

    [Fact]
    public async Task ReceivesNotification()
    {
        using var testContext = this.CreateTestContext( new TestProjectOptions( hasSourceGeneratorTouchFile: true ) );
        var serviceProvider = testContext.ServiceProvider;
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
        var notificationListenerEndpoint = new NotificationListenerEndpoint( serviceProvider, hubPipeName );
        _ = notificationListenerEndpoint.ConnectAsync();

        var notificationReceivedTask = new TaskCompletionSource<CompilationResultChangedEventArgs>();

        notificationListenerEndpoint.CompilationResultChanged += args =>
        {
            notificationReceivedTask.SetResult( args );
            notificationReceivedTask = new TaskCompletionSource<CompilationResultChangedEventArgs>();
        };

        var pipelineFactory = new TestDesignTimeAspectPipelineFactory( testContext, serviceProvider.WithService( analysisProcessServiceHubEndpoint ) );

        var compilation1 = CreateCSharpCompilation( "", name: "project" );
        var pipeline = pipelineFactory.GetOrCreatePipeline( testContext.ProjectOptions, compilation1 ).AssertNotNull();

        // The first pipeline execution should notify a full compilation.
        var initialWait = notificationReceivedTask;
        await pipeline.ExecuteAsync( compilation1 );
        var notification = await initialWait.Task;

        Assert.NotSame( initialWait, notificationReceivedTask );

        Assert.False( notification.IsPartialCompilation );

        var compilation2 = CreateCSharpCompilation( "class C{}", name: "project" );
        await pipeline.ExecuteAsync( compilation2 );
        var notification2 = await notificationReceivedTask.Task;

        Assert.True( notification2.IsPartialCompilation );
    }
}