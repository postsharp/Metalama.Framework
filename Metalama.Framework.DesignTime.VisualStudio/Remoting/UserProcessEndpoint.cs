// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Framework.DesignTime.CodeFixes;
using Metalama.Framework.Engine.CodeFixes;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Threading;
using StreamJsonRpc;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO.Pipes;

namespace Metalama.Framework.DesignTime.VisualStudio.Remoting;

/// <summary>
/// Implements the remoting API of the user process.
/// </summary>
internal partial class UserProcessEndpoint : ServiceEndpoint, IDisposable, ICodeRefactoringDiscoveryService, ICodeActionExecutionService
{
    private readonly ILogger _logger;
    private readonly string? _pipeName;
    private readonly ApiImplementation _apiImplementation;
    private readonly ConcurrentDictionary<string, ImmutableDictionary<string, string>> _unhandledSources = new();
    private readonly ConcurrentDictionary<string, IProjectHandlerCallback> _projectHandlers = new();
    private readonly TaskCompletionSource<bool> _connectTask = new();

    private NamedPipeClientStream? _pipeStream;
    private JsonRpc? _rpc;
    private IAnalysisProcessApi? _server;

    public UserProcessEndpoint( IServiceProvider serviceProvider, string? pipeName = null )
    {
        this._logger = serviceProvider.GetLoggerFactory().GetLogger( "Remoting" );
        this._pipeName = pipeName ?? GetPipeName();
        this._apiImplementation = new ApiImplementation( this );
    }

    public async Task ConnectAsync( CancellationToken cancellationToken = default )
    {
        this._logger.Trace?.Log( $"Connecting to the ServiceHost '{this._pipeName}'." );

        try
        {
            this._pipeStream = new NamedPipeClientStream( ".", this._pipeName!, PipeDirection.InOut, PipeOptions.Asynchronous );
            await this._pipeStream.ConnectAsync( cancellationToken );

            this._rpc = CreateRpc( this._pipeStream );
            this._rpc.AddLocalRpcTarget<IUserProcessApi>( this._apiImplementation, null );
            this._rpc.AddLocalRpcTarget<IProjectHandlerCallback>( this._apiImplementation, null );
            this._server = this._rpc.Attach<IAnalysisProcessApi>();
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
        await (await this.GetServerApiAsync( cancellationToken )).OnProjectHandlerReadyAsync( projectId, cancellationToken );
    }

    public async ValueTask<IAnalysisProcessApi> GetServerApiAsync( CancellationToken cancellationToken = default )
    {
        await this._connectTask.Task.WithCancellation( cancellationToken );

        return this._server ?? throw new InvalidOperationException();
    }

    private static string GetPipeName() => AnalysisProcessEndpoint.GetPipeName( Process.GetCurrentProcess().Id );

    public bool TryGetUnhandledSources( string projectId, out ImmutableDictionary<string, string>? sources )
        => this._unhandledSources.TryRemove( projectId, out sources );

    public void Dispose()
    {
        this._rpc?.Dispose();
        this._pipeStream?.Dispose();
    }

    public event Action<bool>? IsEditingCompileTimeCodeChanged;

    async Task<ComputeRefactoringResult> ICodeRefactoringDiscoveryService.ComputeRefactoringsAsync(
        string projectId,
        string syntaxTreePath,
        TextSpan span,
        CancellationToken cancellationToken )
    {
        var peer = await this.GetServerApiAsync( cancellationToken );

        return await peer.ComputeRefactoringsAsync(
            projectId,
            syntaxTreePath,
            span,
            cancellationToken );
    }

    async Task<CodeActionResult> ICodeActionExecutionService.ExecuteCodeActionAsync(
        string projectId,
        CodeActionModel codeActionModel,
        CancellationToken cancellationToken )
    {
        var peer = await this.GetServerApiAsync( cancellationToken );

        return await peer.ExecuteCodeActionAsync( projectId, codeActionModel, cancellationToken );
    }
}