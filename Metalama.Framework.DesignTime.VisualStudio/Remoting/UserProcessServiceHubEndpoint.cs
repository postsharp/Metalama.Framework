// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

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
    private readonly ConcurrentDictionary<string, TaskCompletionSource<UserProcessEndpoint>> _waiters = new( StringComparer.Ordinal );
    private readonly ConcurrentDictionary<string, UserProcessEndpoint> _registeredEndpointsByPipeName = new( StringComparer.Ordinal );
    private readonly ConcurrentDictionary<string, UserProcessEndpoint> _registeredEndpointsByProjectId = new( StringComparer.Ordinal );

    public UserProcessServiceHubEndpoint( IServiceProvider serviceProvider, string pipeName ) : base( serviceProvider, pipeName )
    {
        this._serviceProvider = serviceProvider;
        this._apiImplementation = new ApiImplementation( this );
    }

    public ICollection<string> RegisteredProjects => this._registeredEndpointsByProjectId.Keys;

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

    private async ValueTask<UserProcessEndpoint> GetEndpointAsync( string projectId, CancellationToken cancellationToken )
    {
        await this.WhenInitialized;

        if ( !this._registeredEndpointsByProjectId.TryGetValue( projectId, out var endpoint ) )
        {
            this.Logger.Warning?.Log( $"The project '{projectId}' is not registered. Waiting." );
            var waiter = this._waiters.GetOrAdd( projectId, _ => new TaskCompletionSource<UserProcessEndpoint>() );
            endpoint = await waiter.Task.WithCancellation( cancellationToken );
        }

        return endpoint;
    }

    public async Task<IAnalysisProcessApi> GetApiAsync( string projectId, CancellationToken cancellationToken )
    {
        var endpoint = await this.GetEndpointAsync( projectId, cancellationToken );

        return await endpoint.GetServerApiAsync( cancellationToken );
    }

    public async Task RegisterProjectHandlerAsync( string projectId, IProjectHandlerCallback callback, CancellationToken cancellationToken = default )
    {
        var endpoint = await this.GetEndpointAsync( projectId, cancellationToken );

        await endpoint.RegisterProjectHandlerAsync( projectId, callback, cancellationToken );
    }

    public bool TryGetUnhandledSources( string projectId, out ImmutableDictionary<string, string>? sources )
    {
        var endpointTask = this.GetEndpointAsync( projectId, CancellationToken.None );

        if ( !endpointTask.IsCompleted )
        {
            sources = null;

            return false;
        }

#pragma warning disable VSTHRD002
        return endpointTask.Result.TryGetUnhandledSources( projectId, out sources );
#pragma warning restore VSTHRD002
    }

    async Task<ComputeRefactoringResult> ICodeRefactoringDiscoveryService.ComputeRefactoringsAsync(
        string projectId,
        string syntaxTreePath,
        TextSpan span,
        CancellationToken cancellationToken )
    {
        var api = await this.GetApiAsync( projectId, cancellationToken );

        return await api.ComputeRefactoringsAsync( projectId, syntaxTreePath, span, cancellationToken );
    }

    async Task<CodeActionResult> ICodeActionExecutionService.ExecuteCodeActionAsync(
        string projectId,
        CodeActionModel codeActionModel,
        CancellationToken cancellationToken )
    {
        var api = await this.GetApiAsync( projectId, cancellationToken );

        return await api.ExecuteCodeActionAsync( projectId, codeActionModel, cancellationToken );
    }

    private void OnIsEditingCompileTimeCodeChanged( bool value )
    {
        this.IsEditingCompileTimeCodeChanged?.Invoke( value );
    }
}