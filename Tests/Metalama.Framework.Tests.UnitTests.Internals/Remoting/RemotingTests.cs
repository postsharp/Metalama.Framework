// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.DesignTime.VisualStudio.Remoting;
using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.Remoting;

public class RemotingTests
{
    [Fact]
    public async Task PublishGeneratedSourceAsync()
    {
        var pipeName = $"Metalama_Test_{Guid.NewGuid()}";
        using var server = new ServiceHost( pipeName );
        using var client = new ServiceClient( pipeName );

        server.Start();
        await client.ConnectAsync();

        var receiveEventTask = new TaskCompletionSource<GeneratedCodeChangedEventArgs>();
        client.GeneratedCodePublished += ( _, args ) => receiveEventTask.SetResult( args );
        await server.PublishGeneratedSourcesAsync( "id", ImmutableDictionary.Create<string, string>().Add( "id", "content" ) );

        Assert.Equal( TaskStatus.RanToCompletion, receiveEventTask.Task.Status );
        var receivedEvent = await receiveEventTask.Task;

        Assert.Equal( "id", receivedEvent.ProjectId );
        Assert.Single( receivedEvent.GeneratedSources, x => x.Key == "id" && x.Value == "content" );
    }
}