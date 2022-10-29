// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.Pipeline;
using Metalama.Framework.Project;
using StreamJsonRpc;
using System.Collections.Concurrent;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.VisualStudio.Remoting;

/// <summary>
/// Implements the remoting API of the analysis process.
/// </summary>
internal partial class AnalysisProcessEndpoint : ServerEndpoint, IService
{
    private static readonly object _initializeLock = new();
    private static AnalysisProcessEndpoint? _instance;

    private readonly ApiImplementation _apiImplementation;
    private readonly ConcurrentDictionary<ProjectKey, ProjectKey> _connectedProjectCallbacks = new();
    private readonly ConcurrentDictionary<ProjectKey, ImmutableDictionary<string, string>> _generatedSourcesForUnconnectedClients = new();
    private readonly IServiceProvider _serviceProvider;

    private readonly ICompileTimeCodeEditingStatusService? _compileTimeCodeEditingStatusService;

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
                    var pipeName = GetPipeName( ServiceRole.Service );
                    _instance = new AnalysisProcessEndpoint( serviceProvider, pipeName );
                    _instance.Start();
                }
            }
        }

        return _instance;
    }

    public AnalysisProcessEndpoint( IServiceProvider serviceProvider, string pipeName ) : base( serviceProvider, pipeName )
    {
        this._compileTimeCodeEditingStatusService = serviceProvider.GetService<ICompileTimeCodeEditingStatusService>();

        if ( this._compileTimeCodeEditingStatusService != null )
        {
            this._compileTimeCodeEditingStatusService.IsEditingCompileTimeCodeChanged += this.OnIsEditingCompileTimeCodeChanged;
        }

        this._serviceProvider = serviceProvider;
        this._apiImplementation = new ApiImplementation( this );
    }

    protected override void ConfigureRpc( JsonRpc rpc )
    {
        rpc.AddLocalRpcTarget<IAnalysisProcessApi>( this._apiImplementation, null );
        this._client = rpc.Attach<IUserProcessApi>();
    }

    protected override async Task OnPipeCreatedAsync( CancellationToken cancellationToken )
    {
        var registrationServiceProvider = this._serviceProvider.GetService<IServiceHubApiProvider>();

        if ( registrationServiceProvider != null )
        {
            this.Logger.Trace?.Log( $"Registering the endpoint '{this.PipeName}' on the hub." );
            var registrationService = await registrationServiceProvider.GetApiAsync( cancellationToken );
            await registrationService.RegisterEndpointAsync( this.PipeName, cancellationToken );
        }
        else
        {
            this.Logger.Warning?.Log( "Hub service not available." );
        }
    }

    private void OnIsEditingCompileTimeCodeChanged( bool isEditing )
    {
        this._client?.OnIsEditingCompileTimeCodeChanged( isEditing );
    }

    public override void Dispose()
    {
        base.Dispose();

        if ( this._compileTimeCodeEditingStatusService != null )
        {
            this._compileTimeCodeEditingStatusService.IsEditingCompileTimeCodeChanged -= this.OnIsEditingCompileTimeCodeChanged;
        }
    }

    public event EventHandler<ClientConnectedEventArgs>? ClientConnected;

    public async Task PublishGeneratedSourcesAsync(
        ProjectKey projectKey,
        ImmutableDictionary<string, string> generatedSources,
        CancellationToken cancellationToken = default )
    {
        void StoreWhenUnconnected()
        {
            Thread.MemoryBarrier();
            this._generatedSourcesForUnconnectedClients[projectKey] = generatedSources;
        }

        if ( !this.WaitUntilInitializedAsync( cancellationToken ).IsCompleted )
        {
            this.Logger.Warning?.Log( $"Cannot publish source for the client '{projectKey}' because the endpoint initialization has not completed." );

            StoreWhenUnconnected();
        }
        else if ( !this._connectedProjectCallbacks.ContainsKey( projectKey ) )
        {
            this.Logger.Warning?.Log( $"Cannot publish source for the client '{projectKey}' because the callback interface has not connected yet." );

            StoreWhenUnconnected();
        }
        else
        {
            this.Logger.Trace?.Log( $"Publishing source for the client '{projectKey}'." );
            await this._client!.PublishGeneratedCodeAsync( projectKey, generatedSources, cancellationToken );
        }
    }

    public async Task RegisterProjectAsync( ProjectKey projectKey )
    {
        await this.WaitUntilInitializedAsync();

        var registrationServiceProvider = this._serviceProvider.GetService<IServiceHubApiProvider>();

        if ( registrationServiceProvider != null )
        {
            this.Logger.Trace?.Log( $"Registering the project '{projectKey}' on the hub." );
            var registrationService = await registrationServiceProvider.GetApiAsync( CancellationToken.None );
            await registrationService.RegisterProjectAsync( projectKey, this.PipeName, CancellationToken.None );
        }
        else
        {
            this.Logger.Trace?.Log( $"The project '{projectKey}' was not registered on the hub because there is no hub service." );
        }
    }
}