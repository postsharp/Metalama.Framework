// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Utilities;
using Metalama.Framework.DesignTime.Pipeline;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Utilities.Diagnostics;
using Metalama.Framework.Project;
using StreamJsonRpc;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO.Pipes;

namespace Metalama.Framework.DesignTime.VisualStudio.Remoting;

/// <summary>
/// Implements the remoting API of the analysis process.
/// </summary>
internal partial class AnalysisProcessEndpoint : ServiceEndpoint, IService, IDisposable
{
    private readonly ILogger _logger;
    private static readonly object _initializeLock = new();
    private static AnalysisProcessEndpoint? _instance;

    private readonly string _pipeName;
    private readonly ApiImplementation _service;
    private readonly CancellationTokenSource _startCancellationSource = new();
    private readonly ConcurrentDictionary<string, string> _connectedClients = new();
    private readonly ConcurrentDictionary<string, ImmutableDictionary<string, string>> _sourcesForUnconnectedClients = new();
    private readonly TaskCompletionSource<bool> _startTask = new();
    private readonly IServiceProvider _serviceProvider;

    private readonly ICompileTimeCodeEditingStatusService? _compileTimeCodeEditingStatusService;

    private NamedPipeServerStream? _pipeStream;
    private JsonRpc? _rpc;
    private IUserProcessApi? _client;

    /// <summary>
    /// Initializes the global instance of the service.
    /// </summary>
    public static AnalysisProcessEndpoint GetInstance( IServiceProvider serviceProvider )
    {
        if ( _instance == null )
        {
            lock ( _initializeLock )
            {
                if ( _instance == null )
                {
                    if ( TryGetPipeName( out var pipeName ) )
                    {
                        _instance = new AnalysisProcessEndpoint( serviceProvider, pipeName );
                        _instance.Start();
                    }
                }
            }
        }

        return _instance!;
    }

    public AnalysisProcessEndpoint( IServiceProvider serviceProvider, string pipeName )
    {
        this._logger = serviceProvider.GetLoggerFactory().GetLogger( "Remoting" );
        this._compileTimeCodeEditingStatusService = serviceProvider.GetService<ICompileTimeCodeEditingStatusService>();

        if ( this._compileTimeCodeEditingStatusService != null )
        {
            this._compileTimeCodeEditingStatusService.IsEditingCompileTimeCodeChanged += this.OnIsEditingCompileTimeCodeChanged;
        }

        this._serviceProvider = serviceProvider;
        this._pipeName = pipeName;
        this._service = new ApiImplementation( this );
    }

    public static string GetPipeName( int processId ) => $"Metalama_{processId}_{EngineAssemblyMetadataReader.Instance.BuildId}";

    private static bool TryGetPipeName( [NotNullWhen( true )] out string? pipeName )
    {
        var parentProcesses = ProcessUtilities.GetParentProcesses();

        Logger.Remoting.Trace?.Log( $"Parent processes: {string.Join( ", ", parentProcesses.Select( x => x.ToString() ) )}" );

        if ( parentProcesses.Count < 3 ||
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

    /// <summary>
    /// Starts the RPC connection, but does not wait until the service is fully started.
    /// </summary>
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

            this._rpc = CreateRpc( this._pipeStream );

            this._rpc.AddLocalRpcTarget<IAnalysisProcessApi>( this._service, null );
            this._client = this._rpc.Attach<IUserProcessApi>();
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

    private void OnIsEditingCompileTimeCodeChanged( bool isEditing )
    {
        this._client?.OnIsEditingCompileTimeCodeChanged( isEditing );
    }

    public void Dispose()
    {
        this._rpc?.Dispose();
        this._pipeStream?.Dispose();

        if ( this._compileTimeCodeEditingStatusService != null )
        {
            this._compileTimeCodeEditingStatusService.IsEditingCompileTimeCodeChanged -= this.OnIsEditingCompileTimeCodeChanged;
        }
    }

    public event EventHandler<ClientConnectedEventArgs>? ClientConnected;

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