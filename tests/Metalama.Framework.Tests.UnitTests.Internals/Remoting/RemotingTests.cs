// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime;
using Metalama.Framework.DesignTime.Contracts;
using Metalama.Framework.DesignTime.Preview;
using Metalama.Framework.DesignTime.VisualStudio.Remoting;
using Metalama.Framework.Engine.Pipeline;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.Remoting;

public class RemotingTests
{
    [Fact]
    public async Task PublishGeneratedSourceAfterHelloAsync()
    {
        var projectKey = ProjectKey.CreateTest( "myProjectId" );
        const string sourceTreeName = "mySource";

        var pipeName = $"Metalama_Test_{Guid.NewGuid()}";
        using var server = new AnalysisProcessEndpoint( ServiceProvider.Empty, pipeName );
        using var client = new UserProcessEndpoint( ServiceProvider.Empty, pipeName );
        var projectHandler = new TestProjectHandler();

        server.Start();
        await client.ConnectAsync();

        await client.RegisterProjectHandlerAsync( projectKey, projectHandler );

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
        using var server = new AnalysisProcessEndpoint( ServiceProvider.Empty, pipeName );
        server.Start();

        // Start the client, but do not call Hello.
        using var client = new UserProcessEndpoint( ServiceProvider.Empty, pipeName );
        var projectHandler = new TestProjectHandler();
        await client.ConnectAsync();

        // Publish from the server.
        await server.PublishGeneratedSourcesAsync( projectKey, ImmutableDictionary.Create<string, string>().Add( sourceTreeName, "content" ) );

        // Finish the connection from the client. We should receive the message that were sent before saying hello.
        await client.RegisterProjectHandlerAsync( projectKey, projectHandler );

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
        using var server = new AnalysisProcessEndpoint( ServiceProvider.Empty, pipeName );
        server.Start();

        // Publish from the server.
        await server.PublishGeneratedSourcesAsync( projectKey, ImmutableDictionary.Create<string, string>().Add( sourceTreeName, "content" ) );

        // Start the client.
        using var client = new UserProcessEndpoint( ServiceProvider.Empty, pipeName );
        var projectHandler = new TestProjectHandler();
        await client.ConnectAsync();
        await client.RegisterProjectHandlerAsync( projectKey, projectHandler );

        // Asserts.
        Assert.Single( projectHandler.GeneratedCodeEvents, x => x.ProjectKey == projectKey );
        Assert.Single( projectHandler.GeneratedCodeEvents[0].Sources, x => x.Key == sourceTreeName );
    }

    [Fact]
    public async Task TransformPreviewAsync()
    {
        // Start the server.
        var pipeName = $"Metalama_Test_{Guid.NewGuid()}";
        using var server = new AnalysisProcessEndpoint( ServiceProvider.Empty.WithService( new PreviewImpl() ), pipeName );
        server.Start();

        using var client = new UserProcessEndpoint( ServiceProvider.Empty, pipeName );
        await client.ConnectAsync();

        var result = await (await client.GetServerApiAsync()).PreviewTransformationAsync(
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
        using var userProcessRegistrationEndpoint = new UserProcessServiceHubEndpoint( ServiceProvider.Empty, discoveryPipeName );
        userProcessRegistrationEndpoint.Start();

        using var analysisProcessRegistrationEndpoint = new AnalysisProcessServiceHubEndpoint( ServiceProvider.Empty, discoveryPipeName );
        _ = analysisProcessRegistrationEndpoint.ConnectAsync();

        var servicePipeName = $"Metalama_Test_Service_{Guid.NewGuid()}";

        using var analysisProcessEndpoint = new AnalysisProcessEndpoint(
            ServiceProvider.Empty.WithService( analysisProcessRegistrationEndpoint ),
            servicePipeName );

        analysisProcessEndpoint.Start();

        var projectKey = ProjectKey.CreateTest( "MyProjectId" );
        await analysisProcessEndpoint.RegisterProjectAsync( projectKey );

        Assert.Contains( projectKey, userProcessRegistrationEndpoint.RegisteredProjects );
    }

    [Fact]
    public async Task RegisterTwoEndpointsAsync()
    {
        var discoveryPipeName = $"Metalama_Test_Discovery_{Guid.NewGuid()}";
        using var userProcessRegistrationEndpoint = new UserProcessServiceHubEndpoint( ServiceProvider.Empty, discoveryPipeName );
        userProcessRegistrationEndpoint.Start();

        using var analysisProcessRegistrationEndpoint = new AnalysisProcessServiceHubEndpoint( ServiceProvider.Empty, discoveryPipeName );
        _ = analysisProcessRegistrationEndpoint.ConnectAsync();

        for ( var i = 0; i < 2; i++ )
        {
            var servicePipeName = $"Metalama_Test_Service_{Guid.NewGuid()}";

            using var analysisProcessEndpoint = new AnalysisProcessEndpoint(
                ServiceProvider.Empty.WithService( analysisProcessRegistrationEndpoint ),
                servicePipeName );

            analysisProcessEndpoint.Start();

            var projectKey = ProjectKey.CreateTest( $"MyProjectId{i}" );
            await analysisProcessEndpoint.RegisterProjectAsync( projectKey );

            Assert.Contains( projectKey, userProcessRegistrationEndpoint.RegisteredProjects );
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