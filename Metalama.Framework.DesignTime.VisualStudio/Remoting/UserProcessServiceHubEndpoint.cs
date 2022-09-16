// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.CodeFixes;
using Metalama.Framework.Engine.CodeFixes;
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

    protected override void ConfigureRpc( JsonRpc rpc )
    {
        rpc.AddLocalRpcTarget<IServiceHubApi>( this._apiImplementation, null );
    }

    private async ValueTask<UserProcessEndpoint> GetEndpointAsync( ProjectKey projectKey, CancellationToken cancellationToken )
    {
        await this.WaitUntilInitializedAsync( cancellationToken );

        if ( !this._registeredEndpointsByProject.TryGetValue( projectKey, out var endpoint ) )
        {
            this.Logger.Warning?.Log( $"The project '{projectKey}' is not registered. Waiting." );
            var waiter = this._waiters.GetOrAdd( projectKey, _ => new TaskCompletionSource<UserProcessEndpoint>() );
            endpoint = await waiter.Task.WithCancellation( cancellationToken );
        }

        return endpoint;
    }

    public async Task<IAnalysisProcessApi> GetApiAsync( ProjectKey projectKey, CancellationToken cancellationToken )
    {
        var endpoint = await this.GetEndpointAsync( projectKey, cancellationToken );

        return await endpoint.GetServerApiAsync( cancellationToken );
    }

    public async Task RegisterProjectCallbackAsync( ProjectKey projectKey, IProjectHandlerCallback callback, CancellationToken cancellationToken = default )
    {
        this.Logger.Trace?.Log( $"Registering '{projectKey}'." );
        var endpoint = await this.GetEndpointAsync( projectKey, cancellationToken );

        await endpoint.RegisterProjectCallbackAsync( projectKey, callback, cancellationToken );

        this.Logger.Trace?.Log( $"Project '{projectKey}' successfully registered." );
    }

    public bool TryGetUnhandledSources( ProjectKey projectKey, out ImmutableDictionary<string, string>? sources )
    {
        var endpointTask = this.GetEndpointAsync( projectKey, CancellationToken.None );

        if ( !endpointTask.IsCompleted )
        {
            sources = null;

            return false;
        }

#pragma warning disable VSTHRD002
        return endpointTask.Result.TryGetUnhandledSources( projectKey, out sources );
#pragma warning restore VSTHRD002
    }

    async Task<ComputeRefactoringResult> ICodeRefactoringDiscoveryService.ComputeRefactoringsAsync(
        ProjectKey projectKey,
        string syntaxTreePath,
        TextSpan span,
        CancellationToken cancellationToken )
    {
        var api = await this.GetApiAsync( projectKey, cancellationToken );

        return await api.ComputeRefactoringsAsync( projectKey, syntaxTreePath, span, cancellationToken );
    }

    async Task<CodeActionResult> ICodeActionExecutionService.ExecuteCodeActionAsync(
        ProjectKey projectKey,
        CodeActionModel codeActionModel,
        CancellationToken cancellationToken )
    {
        var api = await this.GetApiAsync( projectKey, cancellationToken );

        return await api.ExecuteCodeActionAsync( projectKey, codeActionModel, cancellationToken );
    }

    private void OnIsEditingCompileTimeCodeChanged( bool value )
    {
        this.IsEditingCompileTimeCodeChanged?.Invoke( value );
    }
}