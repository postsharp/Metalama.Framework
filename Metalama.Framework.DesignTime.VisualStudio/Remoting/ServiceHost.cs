// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Framework.DesignTime.Preview;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Project;
using StreamJsonRpc;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO.Pipes;

namespace Metalama.Framework.DesignTime.VisualStudio.Remoting;

internal class ServiceHost : IService, IDisposable
{
    private readonly ILogger _logger;
    private static readonly object _initializeLock = new();
    private static ServiceHost? _instance;

    private readonly string? _pipeName;
    private readonly MessageHandler _handler;
    private readonly CancellationTokenSource _startCancellationSource = new();
    private readonly ConcurrentDictionary<string, string> _connectedClients = new();
    private readonly ConcurrentDictionary<string, ImmutableDictionary<string, string>> _sourcesForUnconnectedClients = new();
    private readonly TaskCompletionSource<bool> _startTask = new();
    private readonly IServiceProvider _serviceProvider;

    private NamedPipeServerStream? _pipeStream;
    private JsonRpc? _rpc;
    private IClientApi? _client;

    public static ServiceHost? GetInstance( IServiceProvider serviceProvider )
    {
        if ( _instance == null )
        {
            lock ( _initializeLock )
            {
                if ( _instance == null )
                {
                    if ( TryGetPipeName( out var pipeName ) )
                    {
                        _instance = new ServiceHost( serviceProvider, pipeName );
                        _instance.Start();
                    }
                }
            }
        }

        return _instance;
    }

    public ServiceHost( IServiceProvider serviceProvider, string pipeName )
    {
        this._logger = serviceProvider.GetLoggerFactory().GetLogger( "Remoting" );
        this._serviceProvider = serviceProvider;
        this._pipeName = pipeName;
        this._handler = new MessageHandler( this );
    }

    public static string GetPipeName( int processId ) => $"Metalama_{processId}_{AssemblyMetadataReader.BuildId}";

    private static bool TryGetPipeName( [NotNullWhen( true )] out string? pipeName )
    {
        var parentProcesses = ProcessUtilities.GetParentProcesses();

        Logger.Remoting.Trace?.Log( $"Parent processes: {string.Join( ", ", parentProcesses.Select( x => x.ToString() ) )}" );

        if ( parentProcesses.Length < 3 ||
             !string.Equals( parentProcesses[1].ProcessName, "Microsoft.ServiceHub.Controller", StringComparison.OrdinalIgnoreCase ) ||
             !string.Equals( parentProcesses[2].ProcessName, "devenv", StringComparison.OrdinalIgnoreCase )
           )
        {
            Logger.Remoting.Error?.Log( "The process 'devenv' could not be found. " );
            pipeName = null;

            return false;
        }

        var parentProcess = parentProcesses[2];

        pipeName = GetPipeName( parentProcess.ProcessId );

        return true;
    }

    public void Start()
    {
        _ = Task.Run( () => this.StartAsync( this._startCancellationSource.Token ) );
    }

    private async Task StartAsync( CancellationToken cancellationToken = default )
    {
        this._logger.Trace?.Log( $"Starting the ServiceHost '{this._pipeName}'." );

        try
        {
            this._pipeStream = new NamedPipeServerStream( this._pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous );
            await this._pipeStream.WaitForConnectionAsync( cancellationToken );

            this._rpc = new JsonRpc( this._pipeStream );
            this._rpc.AddLocalRpcTarget<IServerApi>( this._handler, null );
            this._client = this._rpc.Attach<IClientApi>();
            this._rpc.StartListening();

            this._logger.Trace?.Log( $"The ServiceHost '{this._pipeName}' is ready." );
            this._startTask.SetResult( true );
        }
        catch ( Exception e )
        {
            this._startTask.SetException( e );
            this._logger.Error?.Log( "Cannot start the ServiceHost: " + e );

            throw;
        }
    }

    public void Dispose()
    {
        this._rpc?.Dispose();
        this._pipeStream?.Dispose();
    }

    public event EventHandler<ClientConnectedEventArgs>? ClientConnected;

    private class MessageHandler : IServerApi
    {
        private readonly ServiceHost _parent;

        public MessageHandler( ServiceHost parent )
        {
            this._parent = parent;
        }

        public async Task HelloAsync( string projectId, CancellationToken cancellationToken )
        {
            this._parent._logger.Trace?.Log( $"The client '{projectId}' has connected." );

            this._parent._connectedClients[projectId] = projectId;

            Thread.MemoryBarrier();

            // If we received source before the client connected, publish it for the client now.
            if ( this._parent._sourcesForUnconnectedClients.TryRemove( projectId, out var sources ) )
            {
                this._parent._logger.Trace?.Log( $"Publishing source for the client '{projectId}'." );

                await this._parent._client!.PublishGeneratedCodeAsync( projectId, sources, cancellationToken );
            }

            this._parent.ClientConnected?.Invoke( this._parent, new ClientConnectedEventArgs( projectId ) );
        }

        public Task<PreviewTransformationResult> PreviewTransformationAsync( string projectId, string syntaxTreeName, CancellationToken cancellationToken )
        {
            var implementation = this._parent._serviceProvider.GetRequiredService<TransformationPreviewServiceImpl>();

            return implementation.PreviewTransformationAsync( projectId, syntaxTreeName, cancellationToken );
        }
    }

    public async Task PublishGeneratedSourcesAsync(
        string projectId,
        ImmutableDictionary<string, string> generatedSources,
        CancellationToken cancellationToken = default )
    {
        if ( this._startTask.Task.IsCompleted && this._connectedClients.ContainsKey( projectId ) )
        {
            this._logger.Trace?.Log( $"Publishing source for the client '{projectId}'." );
            await this._client!.PublishGeneratedCodeAsync( projectId, generatedSources, cancellationToken );
        }
        else
        {
            Thread.MemoryBarrier();

            this._logger.Trace?.Log( $"Cannot publish source for the client '{projectId}' because it has not connected yet." );
            this._sourcesForUnconnectedClients[projectId] = generatedSources;
        }
    }
}