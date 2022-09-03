// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

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
    private readonly ConcurrentDictionary<string, string> _connectedClients = new();
    private readonly ConcurrentDictionary<string, ImmutableDictionary<string, string>> _sourcesForUnconnectedClients = new();
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
            var registrationService = await registrationServiceProvider.GetApiAsync( cancellationToken );
            await registrationService.RegisterEndpointAsync( this.PipeName, cancellationToken );
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
        string projectId,
        ImmutableDictionary<string, string> generatedSources,
        CancellationToken cancellationToken = default )
    {
        if ( this.WhenInitialized.IsCompleted && this._connectedClients.ContainsKey( projectId ) )
        {
            this.Logger.Trace?.Log( $"Publishing source for the client '{projectId}'." );
            await this._client!.PublishGeneratedCodeAsync( projectId, generatedSources, cancellationToken );
        }
        else
        {
            Thread.MemoryBarrier();

            this.Logger.Trace?.Log( $"Cannot publish source for the client '{projectId}' because it has not connected yet." );
            this._sourcesForUnconnectedClients[projectId] = generatedSources;
        }
    }

    public void RegisterProject( string projectId ) => _ = this.RegisterProjectAsync( projectId );

    public async Task RegisterProjectAsync( string projectId )
    {
        await this.WhenInitialized;

        var registrationServiceProvider = this._serviceProvider.GetService<IServiceHubApiProvider>();

        if ( registrationServiceProvider != null )
        {
            var registrationService = await registrationServiceProvider.GetApiAsync( CancellationToken.None );
            await registrationService.RegisterProjectAsync( projectId, this.PipeName, CancellationToken.None );
        }
    }
}