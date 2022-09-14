// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.CodeFixes;
using Metalama.Framework.Engine.CodeFixes;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Threading;
using StreamJsonRpc;
using System.Collections.Concurrent;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.VisualStudio.Remoting;

/// <summary>
/// Implements the remoting API of the user process.
/// </summary>
internal partial class UserProcessEndpoint : ClientEndpoint<IAnalysisProcessApi>, ICodeRefactoringDiscoveryService, ICodeActionExecutionService
{
    private readonly ApiImplementation _apiImplementation;
    private readonly ConcurrentDictionary<ProjectKey, ImmutableDictionary<string, string>> _unhandledSources = new();
    private readonly ConcurrentDictionary<ProjectKey, IProjectHandlerCallback> _projectHandlers = new();

    public UserProcessEndpoint( IServiceProvider serviceProvider, string pipeName ) : base( serviceProvider, pipeName )
    {
        this._apiImplementation = new ApiImplementation( this );
    }

    protected override void ConfigureRpc( JsonRpc rpc )
    {
        rpc.AddLocalRpcTarget<IUserProcessApi>( this._apiImplementation, null );
        rpc.AddLocalRpcTarget<IProjectHandlerCallback>( this._apiImplementation, null );
    }

    public async Task RegisterProjectHandlerAsync( ProjectKey projectKey, IProjectHandlerCallback callback, CancellationToken cancellationToken = default )
    {
        await this.WhenInitialized.WithCancellation( cancellationToken );
        this._projectHandlers[projectKey] = callback;
        await (await this.GetServerApiAsync( cancellationToken )).OnUserProcessProjectHandlerConnectedAsync( projectKey, cancellationToken );
    }

    public bool TryGetUnhandledSources( ProjectKey projectKey, out ImmutableDictionary<string, string>? sources )
        => this._unhandledSources.TryRemove( projectKey, out sources );

    public event Action<bool>? IsEditingCompileTimeCodeChanged;

    async Task<ComputeRefactoringResult> ICodeRefactoringDiscoveryService.ComputeRefactoringsAsync(
        ProjectKey projectKey,
        string syntaxTreePath,
        TextSpan span,
        CancellationToken cancellationToken )
    {
        var peer = await this.GetServerApiAsync( cancellationToken );

        return await peer.ComputeRefactoringsAsync(
            projectKey,
            syntaxTreePath,
            span,
            cancellationToken );
    }

    async Task<CodeActionResult> ICodeActionExecutionService.ExecuteCodeActionAsync(
        ProjectKey projectKey,
        CodeActionModel codeActionModel,
        CancellationToken cancellationToken )
    {
        var peer = await this.GetServerApiAsync( cancellationToken );

        return await peer.ExecuteCodeActionAsync( projectKey, codeActionModel, cancellationToken );
    }
}