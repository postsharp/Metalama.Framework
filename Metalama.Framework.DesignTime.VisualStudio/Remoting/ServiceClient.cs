// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Framework.Project;
using Microsoft.VisualStudio.Threading;
using StreamJsonRpc;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO.Pipes;

namespace Metalama.Framework.DesignTime.VisualStudio.Remoting;

internal class ServiceClient : ServiceEndpoint, IDisposable, IService
{
    private readonly ILogger _logger;
    private readonly string? _pipeName;
    private readonly MessageHandler _messageHandler;
    private readonly ConcurrentDictionary<string, ImmutableDictionary<string, string>> _unhandledSources = new();
    private readonly ConcurrentDictionary<string, IProjectHandlerCallback> _projectHandlers = new();
    private readonly TaskCompletionSource<bool> _connectTask = new();

    private NamedPipeClientStream? _pipeStream;
    private JsonRpc? _rpc;
    private IServerApi? _server;

    public ServiceClient( IServiceProvider serviceProvider, string? pipeName = null )
    {
        this._logger = serviceProvider.GetLoggerFactory().GetLogger( "Remoting" );
        this._pipeName = pipeName ?? GetPipeName();
        this._messageHandler = new MessageHandler( this );
    }

    public async Task ConnectAsync( CancellationToken cancellationToken = default )
    {
        this._logger.Trace?.Log( $"Connecting to the ServiceHost '{this._pipeName}'." );

        try
        {
            this._pipeStream = new NamedPipeClientStream( ".", this._pipeName, PipeDirection.InOut, PipeOptions.Asynchronous );
            await this._pipeStream.ConnectAsync( cancellationToken );

            this._rpc = this.CreateRpc( this._pipeStream );
            this._rpc.AddLocalRpcTarget<IClientApi>( this._messageHandler, null );
            this._server = this._rpc.Attach<IServerApi>();
            this._rpc.StartListening();

            this._logger.Trace?.Log( $"The client is connected to the ServiceHost '{this._pipeName}'." );

            this._connectTask.SetResult( true );
        }
        catch ( Exception e )
        {
            this._logger.Error?.Log( $"Cannot connect to the ServiceHost '{this._pipeName}': " + e );

            this._connectTask.SetException( e );

            throw;
        }
    }

    public async Task RegisterProjectHandlerAsync( string projectId, IProjectHandlerCallback callback, CancellationToken cancellationToken = default )
    {
        await this._connectTask.Task.WithCancellation( cancellationToken );
        this._projectHandlers[projectId] = callback;
        await (await this.GetServerApiAsync( cancellationToken )).RegisterProjectHandlerAsync( projectId, cancellationToken );
    }

    public async ValueTask<IServerApi> GetServerApiAsync( CancellationToken cancellationToken = default )
    {
        await this._connectTask.Task.WithCancellation( cancellationToken );

        return this._server ?? throw new InvalidOperationException();
    }

    private static string GetPipeName() => ServiceHost.GetPipeName( Process.GetCurrentProcess().Id );

    public bool TryGetUnhandledSources( string projectId, out ImmutableDictionary<string, string>? sources )
        => this._unhandledSources.TryRemove( projectId, out sources );

    public void Dispose()
    {
        this._pipeStream?.Dispose();
        this._rpc?.Dispose();
    }

    public event Action<bool>? IsEditingCompileTimeCodeChanged;

    private class MessageHandler : IClientApi
    {
        private readonly ServiceClient _parent;

        public MessageHandler( ServiceClient parent )
        {
            this._parent = parent;
        }

        public async Task PublishGeneratedCodeAsync(
            string projectId,
            ImmutableDictionary<string, string> sources,
            CancellationToken cancellationToken = default )
        {
            this._parent._logger.Trace?.Log( $"Received new generated code from the remote host for project '{projectId}'." );

            if ( this._parent._projectHandlers.TryGetValue( projectId, out var client ) )
            {
                await client.PublishGeneratedCodeAsync( projectId, sources, cancellationToken );
            }
            else
            {
                this._parent._logger.Warning?.Log( $"No client registered for project '{projectId}'." );

                // Store the event so that a source generator that would be create later can retrieve it.
                this._parent._unhandledSources[projectId] = sources;
            }
        }

        public void OnIsEditingCompileTimeCodeChanged( bool isEditing )
        {
            this._parent.IsEditingCompileTimeCodeChanged?.Invoke( isEditing );
        }
    }
}