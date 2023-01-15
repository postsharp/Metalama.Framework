// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.Rpc.Notifications;
using Metalama.Framework.Engine;
using Metalama.Framework.Engine.Pipeline.DesignTime;
using Metalama.Testing.UnitTesting;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Framework.Tests.UnitTests.DesignTime;

#pragma warning disable VSTHRD200

public sealed class NotificationIntegrationTests : DistributedDesignTimeTestBase
{
    public NotificationIntegrationTests( ITestOutputHelper logger ) : base( logger ) { }

    [Fact]
    public async Task ReceivesNotification()
    {
        using var testContext = this.CreateDistributedDesignTimeTestContext( null, null, new TestContextOptions() { HasSourceGeneratorTouchFile = true } );

        // Start the notification listener.
        var notificationListenerEndpoint = new NotificationListenerEndpoint(
            testContext.ServiceProvider.Underlying,
            testContext.UserProcessServiceHubEndpoint.PipeName );

        // We need to make sure that the notification listener listens before we run the pipeline,
        // otherwise the notification will be missed.
        await notificationListenerEndpoint.ConnectAsync();
        await testContext.WhenInitialized;

        BlockingCollection<CompilationResultChangedEventArgs> eventQueue = new();

        notificationListenerEndpoint.CompilationResultChanged += eventQueue.Add;

        var compilation1 = TestCompilationFactory.CreateCSharpCompilation( "", name: "project" );
        var pipeline = testContext.PipelineFactory.GetOrCreatePipeline( testContext.ProjectOptions, compilation1 ).AssertNotNull();

        // The first pipeline execution should notify a full compilation.
        await pipeline.ExecuteAsync( compilation1, AsyncExecutionContext.Get() );
        var notification1 = eventQueue.Take( testContext.CancellationToken );

        Assert.False( notification1.IsPartialCompilation );

        var compilation2 = TestCompilationFactory.CreateCSharpCompilation( "class C{}", name: "project" );
        await pipeline.ExecuteAsync( compilation2, AsyncExecutionContext.Get() );
        var notification2 = eventQueue.Take( testContext.CancellationToken );

        Assert.True( notification2.IsPartialCompilation );
    }
}