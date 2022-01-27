// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Utilities;
using StreamJsonRpc;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO.Pipes;

namespace Metalama.Framework.DesignTime.VisualStudio.Remoting;

internal class ServiceClient : IDisposable
{
    private readonly string? _pipeName;
    private readonly MessageHandler _messageHandler;

    private NamedPipeClientStream? _pipeStream;
    private JsonRpc? _rpc;
    private IServerApi? _server;

    public ServiceClient( string? pipeName = null )
    {
        this._pipeName = pipeName ?? GetPipeName();
        this._messageHandler = new MessageHandler( this );
    }

    public async Task ConnectAsync( CancellationToken cancellationToken = default )
    {
        Logger.DesignTime.Trace?.Log( $"Connecting to the ServiceHost '{this._pipeName}'." );

        try
        {
            this._pipeStream = new NamedPipeClientStream( ".", this._pipeName, PipeDirection.InOut, PipeOptions.Asynchronous );
            await this._pipeStream.ConnectAsync( cancellationToken );
            this._rpc = new JsonRpc( this._pipeStream );
            this._rpc.AddLocalRpcTarget<IClientApi>( this._messageHandler, null );
            this._server = this._rpc.Attach<IServerApi>();
            this._rpc.StartListening();

            Logger.DesignTime.Trace?.Log( $"The client is connected to the ServiceHost '{this._pipeName}'." );
        }
        catch ( Exception e )
        {
            Logger.DesignTime.Error?.Log( $"Cannot connect to the ServiceHost '{this._pipeName}': " + e );

            throw;
        }
    }

    public IServerApi ServerApi => this._server ?? throw new InvalidOperationException();

    private static string GetPipeName() => $"Metalama_{Process.GetCurrentProcess().Id}";

    public event EventHandler<GeneratedCodeChangedEventArgs>? GeneratedCodePublished;

    public void Dispose()
    {
        this._pipeStream?.Dispose();
        this._rpc?.Dispose();
    }

    private class MessageHandler : IClientApi
    {
        private readonly ServiceClient _parent;

        public MessageHandler( ServiceClient parent )
        {
            this._parent = parent;
        }

        public Task PublishGeneratedCodeAsync( string projectId, ImmutableDictionary<string, string> sources, CancellationToken cancellationToken = default )
        {
            this._parent.GeneratedCodePublished?.Invoke( projectId, new GeneratedCodeChangedEventArgs( projectId, sources ) );

            return Task.CompletedTask;
        }
    }
}