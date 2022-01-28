// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Framework.Engine.Utilities;
using Microsoft.VisualStudio.Threading;
using StreamJsonRpc;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO.Pipes;

namespace Metalama.Framework.DesignTime.VisualStudio.Remoting;

internal class ServiceClient : IDisposable
{
    private static readonly ILogger _logger = Logger.Remoting;

    private readonly string? _pipeName;
    private readonly MessageHandler _messageHandler;
    private readonly ConcurrentDictionary<string, ImmutableDictionary<string, string>> _unhandledSources = new();
    private readonly TaskCompletionSource<bool> _connectTask = new();

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
        _logger.Trace?.Log( $"Connecting to the ServiceHost '{this._pipeName}'." );

        try
        {
            this._pipeStream = new NamedPipeClientStream( ".", this._pipeName, PipeDirection.InOut, PipeOptions.Asynchronous );
            await this._pipeStream.ConnectAsync( cancellationToken );
            this._rpc = new JsonRpc( this._pipeStream );
            this._rpc.AddLocalRpcTarget<IClientApi>( this._messageHandler, null );
            this._server = this._rpc.Attach<IServerApi>();
            this._rpc.StartListening();

            _logger.Trace?.Log( $"The client is connected to the ServiceHost '{this._pipeName}'." );

            this._connectTask.SetResult( true );
        }
        catch ( Exception e )
        {
            _logger.Error?.Log( $"Cannot connect to the ServiceHost '{this._pipeName}': " + e );

            this._connectTask.SetException( e );

            throw;
        }
    }

    public async Task HelloAsync( string projectId, CancellationToken cancellationToken = default )
    {
        await this._connectTask.Task.WithCancellation( cancellationToken );
        await this.ServerApi.HelloAsync( projectId, cancellationToken );
    }

    public IServerApi ServerApi => this._server ?? throw new InvalidOperationException();

    private static string GetPipeName() => ServiceHost.GetPipeName( Process.GetCurrentProcess().Id );

    public event EventHandler<GeneratedCodeChangedEventArgs>? GeneratedCodePublished;

    public bool TryGetUnhandledSources( string projectId, out ImmutableDictionary<string, string>? sources )
        => this._unhandledSources.TryRemove( projectId, out sources );

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
            Logger.DesignTime.Trace?.Log( $"Received new generated code from the remote host for project '{projectId}'." );

            var args = new GeneratedCodeChangedEventArgs( projectId, sources );
            this._parent.GeneratedCodePublished?.Invoke( projectId, args );

            if ( !args.IsHandled )
            {
                _logger.Warning?.Log( $"Nobody handled the new generated code for project '{projectId}'." );

                // Store the event so that a source generator that would be create later can retrieve it.
                this._parent._unhandledSources[projectId] = sources;
            }

            return Task.CompletedTask;
        }
    }
}