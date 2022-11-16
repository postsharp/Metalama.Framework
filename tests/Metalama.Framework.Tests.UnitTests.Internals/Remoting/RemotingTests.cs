// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime;
using Metalama.Framework.DesignTime.Preview;
using Metalama.Framework.DesignTime.VisualStudio.Remoting;
using Metalama.Framework.Engine.Pipeline;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Framework.Tests.UnitTests.Remoting;

public class RemotingTests : LoggingTestBase
{
    private readonly ServiceProvider _serviceProvider;

    public RemotingTests( ITestOutputHelper testOutputHelper ) : base( testOutputHelper )
    {
        this._serviceProvider = this.AddXunitLogging( ServiceProvider.Empty );
    }

    [Fact]
    public async Task PublishGeneratedSourceAfterHelloAsync()
    {
        var projectKey = ProjectKey.CreateTest( "myProjectId" );
        const string sourceTreeName = "mySource";

        var pipeName = $"Metalama_Test_{Guid.NewGuid()}";
        using var server = new AnalysisProcessEndpoint( this._serviceProvider, pipeName );
        using var client = new UserProcessEndpoint( this._serviceProvider, pipeName );
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
        var projectKey = ProjectKey.CreateTest( "myProjectId" );
        const string sourceTreeName = "mySource";

        // Start the server.
        var pipeName = $"Metalama_Test_{Guid.NewGuid()}";
        using var server = new AnalysisProcessEndpoint( this._serviceProvider, pipeName );
        server.Start();

        // Start the client, but do not call Hello.
        using var client = new UserProcessEndpoint( this._serviceProvider, pipeName );
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
        var projectKey = ProjectKey.CreateTest( "myProjectId" );
        const string sourceTreeName = "mySource";

        // Start the server.
        var pipeName = $"Metalama_Test_{Guid.NewGuid()}";
        using var server = new AnalysisProcessEndpoint( this._serviceProvider, pipeName );
        server.Start();

        // Publish from the server.
        await server.PublishGeneratedSourcesAsync( projectKey, ImmutableDictionary.Create<string, string>().Add( sourceTreeName, "content" ) );

        // Start the client.
        using var client = new UserProcessEndpoint( this._serviceProvider, pipeName );
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
        // Start the server.
        var pipeName = $"Metalama_Test_{Guid.NewGuid()}";
        using var server = new AnalysisProcessEndpoint( this._serviceProvider.WithService( new PreviewImpl() ), pipeName );
        server.Start();

        using var client = new UserProcessEndpoint( this._serviceProvider, pipeName );
        await client.ConnectAsync();

        var result = await (await client.GetServerApiAsync( "test" )).PreviewTransformationAsync(
            ProjectKey.CreateTest( "myProjectId" ),
            "syntaxTreeName",
            CancellationToken.None );

        Assert.True( result.IsSuccessful );
        Assert.Equal( "Transformed code", result.TransformedSourceText );
    }

    [Fact]
    public async Task RegisterEndpointAsync()
    {
        var discoveryPipeName = $"Metalama_Test_Discovery_{Guid.NewGuid()}";
        using var userProcessHubEndpoint = new UserProcessServiceHubEndpoint( this._serviceProvider, discoveryPipeName );
        userProcessHubEndpoint.Start();

        using var processServiceHubEndpoint = new AnalysisProcessServiceHubEndpoint( this._serviceProvider, discoveryPipeName );
        _ = processServiceHubEndpoint.ConnectAsync();

        var servicePipeName = $"Metalama_Test_Service_{Guid.NewGuid()}";

        using var analysisProcessEndpoint = new AnalysisProcessEndpoint(
            this._serviceProvider.WithService( processServiceHubEndpoint ),
            servicePipeName );

        analysisProcessEndpoint.Start();

        var projectKey = ProjectKey.CreateTest( "MyProjectId" );
        await analysisProcessEndpoint.RegisterProjectAsync( projectKey );

        Assert.True( userProcessHubEndpoint.IsProjectRegistered( projectKey ) );
    }

    [Fact]
    public async Task RegisterEndpoint_InvertedOrderAndDelayed()
    {
        var discoveryPipeName = $"Metalama_Test_Discovery_{Guid.NewGuid()}";

        using var processServiceHubEndpoint = new AnalysisProcessServiceHubEndpoint( this._serviceProvider, discoveryPipeName );
        _ = processServiceHubEndpoint.ConnectAsync();

        var servicePipeName = $"Metalama_Test_Service_{Guid.NewGuid()}";

        using var analysisProcessEndpoint = new AnalysisProcessEndpoint(
            this._serviceProvider.WithService( processServiceHubEndpoint ),
            servicePipeName );

        analysisProcessEndpoint.Start();

        await Task.Delay( TimeSpan.FromSeconds( 5 ) );
        using var userProcessHubEndpoint = new UserProcessServiceHubEndpoint( this._serviceProvider, discoveryPipeName );
        userProcessHubEndpoint.Start();

        var projectKey = ProjectKey.CreateTest( "MyProjectId" );
        await analysisProcessEndpoint.RegisterProjectAsync( projectKey );

        Assert.True( userProcessHubEndpoint.IsProjectRegistered( projectKey ) );

        await userProcessHubEndpoint.GetApiAsync( projectKey, "Test", CancellationToken.None );
    }

    [Fact]
    public async Task RegisterTwoEndpoints()
    {
        var discoveryPipeName = $"Metalama_Test_Discovery_{Guid.NewGuid()}";
        using var userProcessHubEndpoint = new UserProcessServiceHubEndpoint( this._serviceProvider, discoveryPipeName );
        userProcessHubEndpoint.Start();

        var disposables = new List<IDisposable>();

        for ( var i = 0; i < 2; i++ )
        {
            var analysisProcessServiceHubEndpoint = new AnalysisProcessServiceHubEndpoint( this._serviceProvider, discoveryPipeName );
            _ = analysisProcessServiceHubEndpoint.ConnectAsync();

            var servicePipeName = $"Metalama_Test_Service_{Guid.NewGuid()}";

            var analysisProcessEndpoint = new AnalysisProcessEndpoint(
                this._serviceProvider.WithService( analysisProcessServiceHubEndpoint ),
                servicePipeName );

            analysisProcessEndpoint.Start();

            var projectKey = ProjectKey.CreateTest( $"MyProjectId{i}" );
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
        var discoveryPipeName = $"Metalama_Test_Discovery_{Guid.NewGuid()}";

        var registerTasks = new List<Task>();
        var projectKeys = new List<ProjectKey>();
        var disposables = new List<IDisposable>();

        for ( var i = 0; i < 2; i++ )
        {
            var analysisProcessServiceHubEndpoint = new AnalysisProcessServiceHubEndpoint( this._serviceProvider, discoveryPipeName );

            disposables.Add( analysisProcessServiceHubEndpoint );

            _ = analysisProcessServiceHubEndpoint.ConnectAsync();

            var servicePipeName = $"Metalama_Test_Service_{Guid.NewGuid()}";

            var analysisProcessEndpoint = new AnalysisProcessEndpoint(
                this._serviceProvider.WithService( analysisProcessServiceHubEndpoint ),
                servicePipeName );

            analysisProcessEndpoint.Start();

            var projectKey = ProjectKey.CreateTest( $"MyProjectId{i}" );

            registerTasks.Add( analysisProcessEndpoint.RegisterProjectAsync( projectKey ) );
            projectKeys.Add( projectKey );
            disposables.Add( analysisProcessEndpoint );
        }

        await Task.Delay( TimeSpan.FromSeconds( 1 ) );

        using var userProcessHubEndpoint = new UserProcessServiceHubEndpoint( this._serviceProvider, discoveryPipeName );
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

        // Dispose.
        disposables.Reverse();

        foreach ( var disposable in disposables )
        {
            disposable.Dispose();
        }
    }

    private class PreviewImpl : ITransformationPreviewServiceImpl
    {
        public Task<PreviewTransformationResult> PreviewTransformationAsync( ProjectKey projectKey, string syntaxTreeName, CancellationToken cancellationToken )
        {
            return Task.FromResult( new PreviewTransformationResult( true, "Transformed code", null ) );
        }
    }

    private class TestProjectHandler : IProjectHandlerCallback
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