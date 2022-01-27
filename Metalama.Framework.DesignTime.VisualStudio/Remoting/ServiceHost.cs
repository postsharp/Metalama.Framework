// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Utilities;
using Microsoft.VisualStudio.Threading;
using StreamJsonRpc;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO.Pipes;

namespace Metalama.Framework.DesignTime.VisualStudio.Remoting;

internal class ServiceHost : IDisposable
{
    private readonly string? _pipeName;
    private readonly MessageHandler _handler;
    private readonly CancellationTokenSource _startCancellationSource = new();

    private NamedPipeServerStream? _pipeStream;
    private JsonRpc? _rpc;
    private IClientApi? _client;
    private Task? _startTask;

    public ServiceHost( string pipeName )
    {
        this._pipeName = pipeName;
        this._handler = new MessageHandler();
    }

    public static bool TryGetPipeName( [NotNullWhen( true )] out string? pipeName )
    {
        var parentProcesses = ProcessUtilities.GetParentProcesses();

        if ( parentProcesses.Length < 2 ||
             !string.Equals( parentProcesses[0].ProcessName, "Microsoft.ServiceHub.Controller", StringComparison.OrdinalIgnoreCase ) ||
             !string.Equals( parentProcesses[1].ProcessName, "devenv", StringComparison.OrdinalIgnoreCase )
           )
        {
            pipeName = null;

            return false;
        }

        var parentProcess = parentProcesses[1];

        pipeName = $"Metalama_{parentProcess.ProcessId}";

        return true;
    }

    public void Start()
    {
        this._startTask = Task.Run( () => this.StartAsync( this._startCancellationSource.Token ) );
    }

    private async Task StartAsync( CancellationToken cancellationToken = default )
    {
        Logger.DesignTime.Trace?.Log( $"Starting the ServiceHost '{this._pipeName}'." );

        try
        {
            this._pipeStream = new NamedPipeServerStream( this._pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous );
            await this._pipeStream.WaitForConnectionAsync( cancellationToken );

            this._rpc = new JsonRpc( this._pipeStream );
            this._rpc.AddLocalRpcTarget<IServerApi>( this._handler, null );
            this._client = this._rpc.Attach<IClientApi>();
            this._rpc.StartListening();

            Logger.DesignTime.Trace?.Log( $"The ServiceHost '{this._pipeName}' is ready." );
        }
        catch ( Exception e )
        {
            Logger.DesignTime.Error?.Log( "Cannot start the ServiceHost: " + e );

            throw;
        }
    }

    public void Dispose()
    {
        this._rpc?.Dispose();
        this._pipeStream?.Dispose();
    }

    private class MessageHandler : IServerApi
    {
        public Task<string> PreviewAsync( string fileName, CancellationToken cancellationToken ) => Task.FromResult( "Preview" );
    }

    public async Task PublishGeneratedSourcesAsync(
        string projectId,
        ImmutableDictionary<string, string> generatedSources,
        CancellationToken cancellationToken = default )
    {
        if ( this._startTask == null )
        {
            throw new InvalidOperationException();
        }

        await this._startTask.WithCancellation( cancellationToken );

        await this._client!.PublishGeneratedCodeAsync( projectId, generatedSources, cancellationToken );
    }
}