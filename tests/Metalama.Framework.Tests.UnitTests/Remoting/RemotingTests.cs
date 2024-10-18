// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime;
using Metalama.Framework.DesignTime.Pipeline;
using Metalama.Framework.DesignTime.Preview;
using Metalama.Framework.DesignTime.Rpc;
using Metalama.Framework.DesignTime.Rpc.Notifications;
using Metalama.Framework.DesignTime.VisualStudio.Remoting.AnalysisProcess;
using Metalama.Framework.DesignTime.VisualStudio.Remoting.Api;
using Metalama.Framework.DesignTime.VisualStudio.Remoting.UserProcess;
using Metalama.Framework.Engine.DesignTime;
using Metalama.Framework.Engine.Services;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Framework.Tests.UnitTests.Remoting;

#pragma warning disable VSTHRD200, VSTHRD103

public sealed class RemotingTests : FrameworkBaseTestClass
{
    public RemotingTests( ITestOutputHelper testOutputHelper ) : base( testOutputHelper ) { }

    protected override void ConfigureServices( IAdditionalServiceCollection services )
    {
        base.ConfigureServices( services );
        services.AddGlobalService( sp => new AnalysisProcessEventHub( sp ) );
    }

    [Fact]
    public async Task PublishGeneratedSourceAfterHelloAsync()
    {
        using var testContext = this.CreateTestContext();
        var serviceProvider = testContext.ServiceProvider;

        var projectKey = ProjectKeyFactory.CreateTest( "myProjectId" );
        const string sourceTreeName = "mySource";

        var pipeName = $"Metalama_Test_{Guid.NewGuid()}";
        using var server = new AnalysisProcessEndpoint( serviceProvider, pipeName );
        using var client = new UserProcessEndpoint( serviceProvider, pipeName );
        var projectHandler = new TestProjectHandler();

        server.Start();
        await client.ConnectAsync();

        await client.RegisterProjectCallbackAsync( projectKey, projectHandler );

        await server.PublishGeneratedSourcesAsync( projectKey, ImmutableDictionary.Create<string, string>().Add( sourceTreeName, "content" ) );

        Assert.Single( projectHandler.GeneratedCodeEvents, x => x.ProjectKey == projectKey );
        Assert.Single( projectHandler.GeneratedCodeEvents[0].Sources, x => x.Key == sourceTreeName );
    }

    [Fact]
    public async Task PublishGeneratedSourceBeforeHelloAsync()
    {
        using var testContext = this.CreateTestContext();
        var serviceProvider = testContext.ServiceProvider;

        var projectKey = ProjectKeyFactory.CreateTest( "myProjectId" );
        const string sourceTreeName = "mySource";

        // Start the server.
        var pipeName = $"Metalama_Test_{Guid.NewGuid()}";
        using var server = new AnalysisProcessEndpoint( serviceProvider, pipeName );
        server.Start();

        // Start the client, but do not call Hello.
        using var client = new UserProcessEndpoint( serviceProvider, pipeName );
        var projectHandler = new TestProjectHandler();
        await client.ConnectAsync();

        // Publish from the server.
        await server.PublishGeneratedSourcesAsync( projectKey, ImmutableDictionary.Create<string, string>().Add( sourceTreeName, "content" ) );

        // Finish the connection from the client. We should receive the message that were sent before saying hello.
        await client.RegisterProjectCallbackAsync( projectKey, projectHandler );

        // Asserts.
        Assert.Single( projectHandler.GeneratedCodeEvents, x => x.ProjectKey == projectKey );
        Assert.Single( projectHandler.GeneratedCodeEvents[0].Sources, x => x.Key == sourceTreeName );
    }

    [Fact]
    public async Task PublishGeneratedSourceBeforeConnectAsync()
    {
        using var testContext = this.CreateTestContext();
        var serviceProvider = testContext.ServiceProvider;

        var projectKey = ProjectKeyFactory.CreateTest( "myProjectId" );
        const string sourceTreeName = "mySource";

        // Start the server.
        var pipeName = $"Metalama_Test_{Guid.NewGuid()}";
        using var server = new AnalysisProcessEndpoint( serviceProvider, pipeName );
        server.Start();

        // Publish from the server.
        await server.PublishGeneratedSourcesAsync( projectKey, ImmutableDictionary.Create<string, string>().Add( sourceTreeName, "content" ) );

        // Start the client.
        using var client = new UserProcessEndpoint( serviceProvider, pipeName );
        var projectHandler = new TestProjectHandler();
        await client.ConnectAsync();
        await client.RegisterProjectCallbackAsync( projectKey, projectHandler );

        // Asserts.
        Assert.Single( projectHandler.GeneratedCodeEvents, x => x.ProjectKey == projectKey );
        Assert.Single( projectHandler.GeneratedCodeEvents[0].Sources, x => x.Key == sourceTreeName );
    }

    [Fact]
    public async Task TransformPreviewAsync()
    {
        using var testContext = this.CreateTestContext( new AdditionalServiceCollection( new PreviewImpl() ) );
        var serviceProvider = testContext.ServiceProvider;

        // Start the server.
        var pipeName = $"Metalama_Test_{Guid.NewGuid()}";
        using var server = new AnalysisProcessEndpoint( serviceProvider, pipeName );
        server.Start();

        using var client = new UserProcessEndpoint( serviceProvider, pipeName );
        await client.ConnectAsync();

        var result = await (await client.GetServerApiAsync( "test" )).PreviewTransformationAsync(
            ProjectKeyFactory.CreateTest( "myProjectId" ),
            "syntaxTreeName" );

        Assert.True( result.IsSuccessful );
        AssertEx.EolInvariantEqual( "class TransformedCode {}", result.TransformedSyntaxTree?.Text );
    }

    [Fact]
    public async Task RegisterEndpointAsync()
    {
        using var testContext = this.CreateTestContext();
        var serviceProvider = testContext.ServiceProvider.Global;

        var discoveryPipeName = $"Metalama_Test_Discovery_{Guid.NewGuid()}";
        using var userProcessHubEndpoint = new UserProcessServiceHubEndpoint( serviceProvider, discoveryPipeName );
        userProcessHubEndpoint.Start();

        using var processServiceHubEndpoint = new AnalysisProcessServiceHubEndpoint( serviceProvider, discoveryPipeName );
        _ = processServiceHubEndpoint.ConnectAsync();

        var servicePipeName = $"Metalama_Test_Service_{Guid.NewGuid()}";

        using var analysisProcessEndpoint = new AnalysisProcessEndpoint(
            serviceProvider.WithService( processServiceHubEndpoint ),
            servicePipeName );

        analysisProcessEndpoint.Start();

        var projectKey = ProjectKeyFactory.CreateTest( "MyProjectId" );
        await analysisProcessEndpoint.RegisterProjectAsync( projectKey );

        Assert.True( userProcessHubEndpoint.IsProjectRegistered( projectKey ) );
    }

    [Fact]
    public async Task RegisterEndpoint_InvertedOrderAndDelayed()
    {
        using var testContext = this.CreateTestContext();
        var serviceProvider = testContext.ServiceProvider.Global;

        var discoveryPipeName = $"Metalama_Test_Discovery_{Guid.NewGuid()}";

        using var processServiceHubEndpoint = new AnalysisProcessServiceHubEndpoint( serviceProvider, discoveryPipeName );
        _ = processServiceHubEndpoint.ConnectAsync();

        var servicePipeName = $"Metalama_Test_Service_{Guid.NewGuid()}";

        using var analysisProcessEndpoint = new AnalysisProcessEndpoint(
            serviceProvider.WithService( processServiceHubEndpoint ),
            servicePipeName );

        analysisProcessEndpoint.Start();

        await Task.Delay( TimeSpan.FromSeconds( 5 ) );
        using var userProcessHubEndpoint = new UserProcessServiceHubEndpoint( serviceProvider, discoveryPipeName );
        userProcessHubEndpoint.Start();

        var projectKey = ProjectKeyFactory.CreateTest( "MyProjectId" );
        await analysisProcessEndpoint.RegisterProjectAsync( projectKey );

        Assert.True( userProcessHubEndpoint.IsProjectRegistered( projectKey ) );

        await userProcessHubEndpoint.GetApiAsync( projectKey, "Test", CancellationToken.None );
    }

    [Fact]
    public async Task RegisterTwoEndpoints()
    {
        using var testContext = this.CreateTestContext();
        var serviceProvider = testContext.ServiceProvider.Global;

        var discoveryPipeName = $"Metalama_Test_Discovery_{Guid.NewGuid()}";
        using var userProcessHubEndpoint = new UserProcessServiceHubEndpoint( serviceProvider, discoveryPipeName );
        userProcessHubEndpoint.Start();

        var disposables = new List<IDisposable>();

        for ( var i = 0; i < 2; i++ )
        {
            var analysisProcessServiceHubEndpoint = new AnalysisProcessServiceHubEndpoint( serviceProvider, discoveryPipeName );
            _ = analysisProcessServiceHubEndpoint.ConnectAsync();

            var servicePipeName = $"Metalama_Test_Service_{Guid.NewGuid()}";

            var analysisProcessEndpoint = new AnalysisProcessEndpoint(
                serviceProvider.WithService( analysisProcessServiceHubEndpoint ),
                servicePipeName );

            analysisProcessEndpoint.Start();

            var projectKey = ProjectKeyFactory.CreateTest( $"MyProjectId{i}" );
            await analysisProcessEndpoint.RegisterProjectAsync( projectKey );

            Assert.True( userProcessHubEndpoint.IsProjectRegistered( projectKey ) );

            disposables.Add( analysisProcessServiceHubEndpoint );
            disposables.Add( analysisProcessEndpoint );
        }

        // Dispose.
        disposables.Reverse();

        foreach ( var disposable in disposables )
        {
            disposable.Dispose();
        }
    }

    [Fact]
    public async Task RegisterTwoEndpoints_InvertedOrder()
    {
        using var testContext = this.CreateTestContext();
        var serviceProvider = testContext.ServiceProvider.Global;

        var discoveryPipeName = $"Metalama_Test_Discovery_{Guid.NewGuid()}";

        var registerTasks = new List<Task>();
        var projectKeys = new List<ProjectKey>();
        var disposables = new List<IDisposable>();

        const int clientCount = 2;

        for ( var i = 0; i < clientCount; i++ )
        {
            var analysisProcessServiceHubEndpoint = new AnalysisProcessServiceHubEndpoint( serviceProvider, discoveryPipeName );

            disposables.Add( analysisProcessServiceHubEndpoint );

            _ = analysisProcessServiceHubEndpoint.ConnectAsync();

            var servicePipeName = $"Metalama_Test_Service_{Guid.NewGuid()}";

            var analysisProcessEndpoint = new AnalysisProcessEndpoint(
                serviceProvider.WithService( analysisProcessServiceHubEndpoint ),
                servicePipeName );

            analysisProcessEndpoint.Start();

            var projectKey = ProjectKeyFactory.CreateTest( $"MyProjectId{i}" );

            registerTasks.Add( analysisProcessEndpoint.RegisterProjectAsync( projectKey ) );
            projectKeys.Add( projectKey );
            disposables.Add( analysisProcessEndpoint );
        }

        await Task.Delay( TimeSpan.FromSeconds( 1 ) );

        using var userProcessHubEndpoint = new UserProcessServiceHubEndpoint( serviceProvider, discoveryPipeName );
        userProcessHubEndpoint.Start();

        await Task.WhenAll( registerTasks );

        foreach ( var projectKey in projectKeys )
        {
            Assert.True( userProcessHubEndpoint.IsProjectRegistered( projectKey ) );
        }

        foreach ( var endPoint in userProcessHubEndpoint.Endpoints )
        {
            // Should not wait forever.
            await endPoint.GetServerApiAsync( "Test" );
        }

        Assert.Equal( clientCount, userProcessHubEndpoint.ClientCount );

        // Dispose.
        disposables.Reverse();

        foreach ( var disposable in disposables )
        {
            disposable.Dispose();
        }
    }

    [Fact]
    public async Task MultipleConnect_Sequential()
    {
        using var testContext = this.CreateTestContext();
        var serviceProvider = testContext.ServiceProvider;

        var discoveryPipeName = $"Metalama_Test_Discovery_{Guid.NewGuid()}";

        // Connect the UserProcess endpoint.
        using var userProcessHubEndpoint = new UserProcessServiceHubEndpoint( serviceProvider, discoveryPipeName );
        userProcessHubEndpoint.Start();

        // Connect the AnalysisService endpoint.
        using var processServiceHubEndpoint = new AnalysisProcessServiceHubEndpoint( serviceProvider, discoveryPipeName );
        Assert.True( await processServiceHubEndpoint.ConnectAsync() );

        // The second connect should not do anything.
        Assert.False( await processServiceHubEndpoint.ConnectAsync() );
    }

    [Fact]
    public async Task MultipleConnect_Concurrent()
    {
        using var testContext = this.CreateTestContext();
        var serviceProvider = testContext.ServiceProvider;

        var discoveryPipeName = $"Metalama_Test_Discovery_{Guid.NewGuid()}";

        // Connect the UserProcess endpoint.
        using var userProcessHubEndpoint = new UserProcessServiceHubEndpoint( serviceProvider, discoveryPipeName );
        userProcessHubEndpoint.Start();

        // Connect the AnalysisService endpoint.
        using var processServiceHubEndpoint = new AnalysisProcessServiceHubEndpoint( serviceProvider, discoveryPipeName );
        var task1 = processServiceHubEndpoint.ConnectAsync();
        var task2 = processServiceHubEndpoint.ConnectAsync();

        await Task.WhenAll( task1, task2 );

        Assert.True( task1.Result );
        Assert.False( task2.Result );
    }

    [Fact]
    public async Task PublishChangeNotification()
    {
        using var testContext = this.CreateTestContext();
        var serviceProvider = testContext.ServiceProvider.Global;

        var discoveryPipeName = $"Metalama_Test_Discovery_{Guid.NewGuid()}";

        // Connect the UserProcess endpoint.
        using var userProcessHubEndpoint = new UserProcessServiceHubEndpoint( serviceProvider, discoveryPipeName );
        userProcessHubEndpoint.Start();

        // Connect the AnalysisService endpoint.
        using var processServiceHubEndpoint = new AnalysisProcessServiceHubEndpoint( serviceProvider, discoveryPipeName );
        _ = processServiceHubEndpoint.ConnectAsync();

        // Connect the CodeLens endpoint.
        using var codeLensEndpoint = new NotificationListenerEndpoint( serviceProvider.Underlying, discoveryPipeName );
        await codeLensEndpoint.ConnectAsync();

        var receivedNotificationTaskSource = new TaskCompletionSource<CompilationResultChangedEventArgs>();
        codeLensEndpoint.CompilationResultChanged += n => receivedNotificationTaskSource.SetResult( n );

        var projectKey = ProjectKeyFactory.CreateTest( "MyProjectId" );
        var sentNotification = new CompilationResultChangedEventArgs( projectKey, false, ImmutableArray<string>.Empty );
        processServiceHubEndpoint.PublishCompilationResultChangedNotification( sentNotification );

        var receivedNotification = await receivedNotificationTaskSource.Task;

        Assert.Equal( sentNotification.ProjectKey, receivedNotification.ProjectKey );
    }

    private sealed class PreviewImpl : ITransformationPreviewServiceImpl
    {
        public Task<SerializablePreviewTransformationResult> PreviewTransformationAsync(
            ProjectKey projectKey,
            string syntaxTreeName,
            CancellationToken cancellationToken )
        {
            return Task.FromResult(
                new SerializablePreviewTransformationResult(
                    true,
                    JsonSerializationHelper.CreateSerializableSyntaxTree( CSharpSyntaxTree.ParseText( "class TransformedCode {}" ) ),
                    null ) );
        }
    }

    private sealed class TestProjectHandler : IProjectHandlerCallbackApi
    {
        public List<(ProjectKey ProjectKey, ImmutableDictionary<string, string> Sources)> GeneratedCodeEvents { get; } = new();

        public Task PublishGeneratedCodeAsync(
            ProjectKey projectKey,
            ImmutableDictionary<string, string> sources,
            CancellationToken cancellationToken = default )
        {
            this.GeneratedCodeEvents.Add( (projectKey, sources) );

            return Task.CompletedTask;
        }
    }
}