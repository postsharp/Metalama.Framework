// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.CodeFixes;
using Metalama.Framework.DesignTime.Contracts.Diagnostics;
using Metalama.Framework.DesignTime.Diagnostics;
using Metalama.Framework.DesignTime.Rpc;
using Metalama.Framework.DesignTime.Rpc.Notifications;
using Metalama.Framework.DesignTime.VisualStudio.Remoting.Api;
using Metalama.Framework.Engine.DesignTime.CodeFixes;
using Metalama.Framework.Engine.Services;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Threading;
using StreamJsonRpc;
using System.Collections.Concurrent;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.VisualStudio.Remoting.UserProcess;

internal sealed partial class UserProcessServiceHubEndpoint : ServerEndpoint, ICodeRefactoringDiscoveryService, ICodeActionExecutionService
{
    private static readonly object _initializeLock = new();
    private static UserProcessServiceHubEndpoint? _instance;

    private readonly GlobalServiceProvider _serviceProvider;
    private readonly ConcurrentDictionary<ProjectKey, TaskCompletionSource<UserProcessEndpoint>> _waiters = new();
    private readonly ConcurrentDictionary<string, UserProcessEndpoint> _registeredEndpointsByPipeName = new( StringComparer.Ordinal );
    private readonly ConcurrentDictionary<ProjectKey, UserProcessEndpoint> _registeredEndpointsByProject = new();
    private readonly ConcurrentDictionary<JsonRpc, ApiImplementation> _clients = new();

    // We use an immutable dictionary to have a simple consistent enumerator.
    private volatile ImmutableDictionary<ProjectKey, ImmutableArray<IDiagnosticData>> _compileTimeErrorsByProject =
        ImmutableDictionary<ProjectKey, ImmutableArray<IDiagnosticData>>.Empty;

    public UserProcessServiceHubEndpoint( GlobalServiceProvider serviceProvider, string pipeName ) : base(
        serviceProvider.Underlying,
        pipeName,
        int.MaxValue,
        JsonSerializationBinderFactory.Instance )
    {
        this._serviceProvider = serviceProvider;
    }

    public bool IsProjectRegistered( ProjectKey projectKey ) => this._registeredEndpointsByProject.ContainsKey( projectKey );

    public ICollection<UserProcessEndpoint> Endpoints => this._registeredEndpointsByPipeName.Values;

    public static UserProcessServiceHubEndpoint GetInstance( GlobalServiceProvider serviceProvider )
    {
        if ( _instance == null )
        {
            lock ( _initializeLock )
            {
                if ( _instance == null )
                {
                    var pipeName = PipeNameProvider.GetPipeName( ServiceRole.Discovery );
                    _instance = new UserProcessServiceHubEndpoint( serviceProvider, pipeName );
                    _instance.Start();
                }
            }
        }

        return _instance;
    }

    public event Action<bool>? IsEditingCompileTimeCodeChanged;

    public event Action<IReadOnlyCollection<IDiagnosticData>>? CompileTimeErrorsChanged;

    public IReadOnlyCollection<IDiagnosticData> CompileTimeErrors { get; private set; } = Array.Empty<DiagnosticData>();

    public event Action<UserProcessEndpoint>? EndpointAdded;

    public event Action<ProjectKey>? AspectClassesChanged;

    public event Action<ProjectKey>? AspectInstancesChanged;

    protected override void ConfigureRpc( JsonRpc rpc )
    {
        var client = new ApiImplementation( this, rpc );
        rpc.AddLocalRpcTarget<IServiceHubApi>( client, null );
        rpc.AddLocalRpcTarget<INotificationHubApi>( client, null );
        rpc.AddLocalRpcTarget<INotificationListenerApi>( client, null );
        this._clients.TryAdd( rpc, client );
    }

    protected override void OnRpcDisconnected( JsonRpc rpc )
    {
        base.OnRpcDisconnected( rpc );
        this._clients.TryRemove( rpc, out _ );
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

    public async Task RegisterProjectCallbackAsync( ProjectKey projectKey, IProjectHandlerCallbackApi callback, CancellationToken cancellationToken = default )
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

    private void OnIsEditingCompileTimeCodeChanged( bool value ) => this.IsEditingCompileTimeCodeChanged?.Invoke( value );

    private void OnCompileTimeErrorsChanged( ImmutableDictionary<ProjectKey, ImmutableArray<IDiagnosticData>> errors )
        => this.SetCompileTimeErrorsForProjects( errors );

    private void OnAspectClassesChanged( ProjectKey projectKey ) => this.AspectClassesChanged?.Invoke( projectKey );

    private void OnAspectInstancesChanged( ProjectKey projectKey ) => this.AspectInstancesChanged?.Invoke( projectKey );

    public event Action<CompilationResultChangedEventArgs>? CompilationResultChanged;

    private Task NotifyCompilationResultChangeAsync( CompilationResultChangedEventArgs notification, CancellationToken cancellationToken )
    {
        this.CompilationResultChanged?.Invoke( notification );

        var tasks = new List<Task>();

        var hasClient = false;

        foreach ( var client in this._clients.Values )
        {
            var listener = client.NotificationListener;

            if ( listener != null )
            {
                hasClient = true;
                this.Logger.Trace?.Log( $"Distributing change notification to client." );
                var task = listener.NotifyCompilationResultChangedAsync( notification, cancellationToken );

                if ( !task.IsCompleted )
                {
                    tasks.Add( task );
                }
            }
        }

        if ( !hasClient )
        {
            this.Logger.Trace?.Log( "No client registered to receive change notifications." );
        }

        if ( tasks.Count > 0 )
        {
            return Task.WhenAll( tasks ).WithCancellation( cancellationToken );
        }
        else
        {
            return Task.CompletedTask;
        }
    }

    private void SetCompileTimeErrorsForProjects( ImmutableDictionary<ProjectKey, ImmutableArray<IDiagnosticData>> errors )
    {
        while ( true )
        {
            var oldDictionary = this._compileTimeErrorsByProject;
            var dictionary = oldDictionary;

            foreach ( var group in errors )
            {
                dictionary = dictionary.SetItem( group.Key, group.Value );
            }

            if ( Interlocked.CompareExchange( ref this._compileTimeErrorsByProject, dictionary, oldDictionary ) == oldDictionary )
            {
                this.CompileTimeErrors = new ConsolidatedErrorDiagnosticCollection( this._compileTimeErrorsByProject );
                this.CompileTimeErrorsChanged?.Invoke( this.CompileTimeErrors );

                return;
            }
        }
    }

    private async Task OnEndpointAddedAsync( UserProcessEndpoint endpoint, CancellationToken cancellationToken )
    {
        this.EndpointAdded?.Invoke( endpoint );

        var compileTimeErrors = await endpoint.GetCompileTimeErrorsAsync( cancellationToken );
        this.SetCompileTimeErrorsForProjects( compileTimeErrors );
    }
}