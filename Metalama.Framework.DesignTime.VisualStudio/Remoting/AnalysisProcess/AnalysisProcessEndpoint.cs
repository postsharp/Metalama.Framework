// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.CodeFixes;
using Metalama.Framework.DesignTime.Diagnostics;
using Metalama.Framework.DesignTime.Pipeline;
using Metalama.Framework.DesignTime.Rpc;
using Metalama.Framework.DesignTime.VisualStudio.Remoting.Api;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Services;
using StreamJsonRpc;
using System.Collections.Concurrent;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.VisualStudio.Remoting.AnalysisProcess;

/// <summary>
/// Implements the remoting API of the analysis process.
/// </summary>
internal sealed partial class AnalysisProcessEndpoint : ServerEndpoint, IGlobalService
{
    private static readonly object _initializeLock = new();
    private static AnalysisProcessEndpoint? _instance;

    private readonly ConcurrentDictionary<ProjectKey, ProjectKey> _connectedProjectCallbacks = new();
    private readonly ConcurrentDictionary<ProjectKey, ImmutableDictionary<string, string>> _generatedSourcesForUnconnectedClients = new();
    private readonly GlobalServiceProvider _serviceProvider;

    private readonly ConcurrentDictionary<JsonRpc, IUserProcessApi> _clients = new();

    private readonly AnalysisProcessEventHub _eventHub;

    private bool _isHubRegistrationProcessed;

    /// <summary>
    /// Initializes the global instance of the service.
    /// </summary>
    public static AnalysisProcessEndpoint GetInstance( GlobalServiceProvider serviceProvider )
    {
        if ( _instance == null )
        {
            lock ( _initializeLock )
            {
                if ( _instance == null )
                {
                    var pipeName = PipeNameProvider.GetPipeName( ServiceRole.Service );
                    _instance = new AnalysisProcessEndpoint( serviceProvider, pipeName );
                    _instance.Start();
                }
            }
        }

        return _instance;
    }

    public AnalysisProcessEndpoint( GlobalServiceProvider serviceProvider, string pipeName ) : base( serviceProvider.Underlying, pipeName, 1 )
    {
        this._serviceProvider = serviceProvider;
        this._eventHub = serviceProvider.GetRequiredService<AnalysisProcessEventHub>();
        this._eventHub.IsEditingCompileTimeCodeChanged += this.OnIsEditingCompileTimeCodeChanged;
        this._eventHub.CompileTimeErrorsChanged += this.OnCompileTimeErrorsChanged;
    }

    protected override void ConfigureRpc( JsonRpc rpc )
    {
        var client = rpc.Attach<IUserProcessApi>();
        var implementation = new ApiImplementation( this, client );
        rpc.AddLocalRpcTarget<IAnalysisProcessApi>( implementation, null );
        rpc.AddLocalRpcTarget<ICodeRefactoringDiscoveryService>( implementation, null );
        rpc.AddLocalRpcTarget<ICodeActionExecutionService>( implementation, null );
        this._clients.TryAdd( rpc, client );
    }

    protected override void OnRpcDisconnected( JsonRpc rpc )
    {
        base.OnRpcDisconnected( rpc );

        this._clients.TryRemove( rpc, out _ );
    }

    protected override async Task OnServerPipeCreatedAsync( CancellationToken cancellationToken )
    {
        // We must connect to the service hub here and now, otherwise the caller would wait forever for a client.

        if ( this._isHubRegistrationProcessed )
        {
            this.Logger.Trace?.Log( $"Registering '{this.PipeName}' to the hub has already been done." );

            return;
        }

        this._isHubRegistrationProcessed = true;

        var registrationServiceProvider = this._serviceProvider.GetService<IServiceHubApiProvider>();

        if ( registrationServiceProvider != null )
        {
            this.Logger.Trace?.Log( $"Registering the endpoint '{this.PipeName}' on the hub." );
            var registrationService = await registrationServiceProvider.GetApiAsync( nameof(this.OnServerPipeCreatedAsync), cancellationToken );
            await registrationService.RegisterAnalysisServiceAsync( this.PipeName, cancellationToken );
            this.Logger.Trace?.Log( $"Registering the endpoint '{this.PipeName}' on the hub: completed." );
        }
        else
        {
            this.Logger.Warning?.Log( "Hub service not available." );
        }
    }

    private void OnIsEditingCompileTimeCodeChanged( bool isEditing )
    {
        foreach ( var client in this._clients.Values )
        {
            client.OnIsEditingCompileTimeCodeChanged( isEditing );
        }
    }

    private void OnCompileTimeErrorsChanged( ProjectKey projectKey, IReadOnlyCollection<DiagnosticData> errors )
    {
        foreach ( var client in this._clients.Values )
        {
            client.OnCompileTimeErrorsChanged( projectKey, errors );
        }
    }

    public override void Dispose()
    {
        base.Dispose();

        this._eventHub.IsEditingCompileTimeCodeChanged -= this.OnIsEditingCompileTimeCodeChanged;
        this._eventHub.CompileTimeErrorsChanged -= this.OnCompileTimeErrorsChanged;
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

        if ( !this.WaitUntilInitializedAsync( nameof(this.PublishGeneratedSourcesAsync), cancellationToken ).IsCompleted )
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

            foreach ( var client in this._clients.Values )
            {
                await client.PublishGeneratedCodeAsync( projectKey, generatedSources, cancellationToken );
            }
        }
    }

    public async Task RegisterProjectAsync( ProjectKey projectKey )
    {
        await this.WaitUntilInitializedAsync( nameof(this.RegisterProjectAsync) );

        var registrationServiceProvider = this._serviceProvider.GetService<IServiceHubApiProvider>();

        if ( registrationServiceProvider != null )
        {
            this.Logger.Trace?.Log( $"Registering the project '{projectKey}' on the hub." );
            var registrationService = await registrationServiceProvider.GetApiAsync( nameof(this.RegisterProjectAsync), CancellationToken.None );
            await registrationService.RegisterAnalysisServiceProjectAsync( projectKey, this.PipeName, CancellationToken.None );
        }
        else
        {
            this.Logger.Trace?.Log( $"The project '{projectKey}' was not registered on the hub because there is no hub service." );
        }
    }
}