// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.CodeFixes;
using Metalama.Framework.Engine.CodeFixes;
using Metalama.Framework.Engine.Collections;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Threading;
using StreamJsonRpc;
using System.Collections.Concurrent;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.VisualStudio.Remoting;

internal partial class UserProcessServiceHubEndpoint : ServerEndpoint, ICodeRefactoringDiscoveryService, ICodeActionExecutionService
{
    private static readonly object _initializeLock = new();
    private static UserProcessServiceHubEndpoint? _instance;

    private readonly IServiceProvider _serviceProvider;
    private readonly ApiImplementation _apiImplementation;
    private readonly ConcurrentDictionary<ProjectKey, TaskCompletionSource<UserProcessEndpoint>> _waiters = new();
    private readonly ConcurrentDictionary<string, UserProcessEndpoint> _registeredEndpointsByPipeName = new( StringComparer.Ordinal );
    private readonly ConcurrentDictionary<ProjectKey, UserProcessEndpoint> _registeredEndpointsByProject = new();

    public UserProcessServiceHubEndpoint( IServiceProvider serviceProvider, string pipeName ) : base( serviceProvider, pipeName )
    {
        this._serviceProvider = serviceProvider;

        // The hub implementation object is shared by all clients.
        this._apiImplementation = new ApiImplementation( this );
    }

    public bool IsProjectRegistered( ProjectKey projectKey ) => this._registeredEndpointsByProject.ContainsKey( projectKey );

    public ICollection<UserProcessEndpoint> Endpoints => this._registeredEndpointsByPipeName.Values;

    public static UserProcessServiceHubEndpoint GetInstance( IServiceProvider serviceProvider )
    {
        if ( _instance == null )
        {
            lock ( _initializeLock )
            {
                if ( _instance == null )
                {
                    var pipeName = GetPipeName( ServiceRole.Discovery );
                    _instance = new UserProcessServiceHubEndpoint( serviceProvider, pipeName );
                    _instance.Start();
                }
            }
        }

        return _instance;
    }

    public event Action<bool>? IsEditingCompileTimeCodeChanged;

    public event Action<UserProcessEndpoint>? EndpointAdded;

    protected override void ConfigureRpc( JsonRpc rpc )
    {
        rpc.AddLocalRpcTarget<IServiceHubApi>( this._apiImplementation, null );
    }

    private async ValueTask<UserProcessEndpoint> GetEndpointAsync( ProjectKey projectKey, string callerName, CancellationToken cancellationToken )
    {
        if ( !projectKey.IsMetalamaEnabled )
        {
            throw new ArgumentOutOfRangeException(
                nameof(projectKey),
                $"Cannot get the endpoint of '{projectKey}' because Metalama is not enabled for this project." );
        }

        await this.WaitUntilInitializedAsync( callerName, cancellationToken );

        if ( !this._registeredEndpointsByProject.TryGetValue( projectKey, out var endpoint ) )
        {
            this.Logger.Warning?.Log( $"The project '{projectKey}' is not registered. Waiting." );
            var waiter = this._waiters.GetOrAddNew( projectKey );
            endpoint = await waiter.Task.WithCancellation( cancellationToken );
            this.Logger.Trace?.Log( $"The project '{projectKey}' is now registered. Resuming." );
        }

        return endpoint;
    }

    public async Task<IAnalysisProcessApi> GetApiAsync( ProjectKey projectKey, string callerName, CancellationToken cancellationToken )
    {
        var endpoint = await this.GetEndpointAsync( projectKey, nameof(this.GetApiAsync), cancellationToken );

        return await endpoint.GetServerApiAsync( callerName, cancellationToken );
    }

    public async Task RegisterProjectCallbackAsync( ProjectKey projectKey, IProjectHandlerCallback callback, CancellationToken cancellationToken = default )
    {
        this.Logger.Trace?.Log( $"Registering callback for '{projectKey}'." );
        var endpoint = await this.GetEndpointAsync( projectKey, nameof(this.RegisterProjectCallbackAsync), cancellationToken );

        await endpoint.RegisterProjectCallbackAsync( projectKey, callback, cancellationToken );

        this.Logger.Trace?.Log( $"Project '{projectKey}' successfully registered." );
    }

    /// <summary>
    /// Gets the generate sources if they are available, but do not wait for them if they are not.
    /// </summary>
    public bool TryGetGenerateSourcesIfAvailable( ProjectKey projectKey, out ImmutableDictionary<string, string>? sources )
    {
        var endpointTask = this.GetEndpointAsync( projectKey, nameof(this.TryGetGenerateSourcesIfAvailable), CancellationToken.None );

        if ( !endpointTask.IsCompleted )
        {
            this.Logger.Warning?.Log( $"TryGetGenerateSourcesIfAvailable('{projectKey}'): endpoint not ready." );
            sources = null;

            return false;
        }

#pragma warning disable VSTHRD002
        if ( !endpointTask.Result.TryGetCachedGeneratedSources( projectKey, out sources ) )
#pragma warning restore VSTHRD002
        {
            this.Logger.Warning?.Log( $"TryGetGenerateSourcesIfAvailable('{projectKey}'): no result is available in cache." );
            sources = null;

            return false;
        }

        return true;
    }

    async Task<ComputeRefactoringResult> ICodeRefactoringDiscoveryService.ComputeRefactoringsAsync(
        ProjectKey projectKey,
        string syntaxTreePath,
        TextSpan span,
        CancellationToken cancellationToken )
    {
        var api = await this.GetApiAsync( projectKey, nameof(ICodeRefactoringDiscoveryService.ComputeRefactoringsAsync), cancellationToken );

        return await api.ComputeRefactoringsAsync( projectKey, syntaxTreePath, span, cancellationToken );
    }

    async Task<CodeActionResult> ICodeActionExecutionService.ExecuteCodeActionAsync(
        ProjectKey projectKey,
        CodeActionModel codeActionModel,
        bool isComputingPreview,
        CancellationToken cancellationToken )
    {
        var api = await this.GetApiAsync( projectKey, nameof(ICodeActionExecutionService.ExecuteCodeActionAsync), cancellationToken );

        return await api.ExecuteCodeActionAsync( projectKey, codeActionModel, isComputingPreview, cancellationToken );
    }

    private void OnIsEditingCompileTimeCodeChanged( bool value )
    {
        this.IsEditingCompileTimeCodeChanged?.Invoke( value );
    }
}