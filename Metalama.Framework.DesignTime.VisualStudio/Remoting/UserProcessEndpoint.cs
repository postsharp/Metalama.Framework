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

/// <summary>
/// Implements the remoting API of the user process.
/// </summary>
internal partial class UserProcessEndpoint : ClientEndpoint<IAnalysisProcessApi>, ICodeRefactoringDiscoveryService, ICodeActionExecutionService
{
    private readonly ApiImplementation _apiImplementation;
    private readonly ConcurrentDictionary<string, ImmutableDictionary<string, string>> _unhandledSources = new();
    private readonly ConcurrentDictionary<string, IProjectHandlerCallback> _projectHandlers = new();

    public UserProcessEndpoint( IServiceProvider serviceProvider, string pipeName ) : base( serviceProvider, pipeName )
    {
        this._apiImplementation = new ApiImplementation( this );
    }

    protected override void ConfigureRpc( JsonRpc rpc )
    {
        rpc.AddLocalRpcTarget<IUserProcessApi>( this._apiImplementation, null );
        rpc.AddLocalRpcTarget<IProjectHandlerCallback>( this._apiImplementation, null );
    }

    public async Task RegisterProjectHandlerAsync( string projectId, IProjectHandlerCallback callback, CancellationToken cancellationToken = default )
    {
        await this.WhenInitialized.WithCancellation( cancellationToken );
        this._projectHandlers[projectId] = callback;
        await (await this.GetServerApiAsync( cancellationToken )).OnUserProcessProjectHandlerConnectedAsync( projectId, cancellationToken );
    }

    public bool TryGetUnhandledSources( string projectId, out ImmutableDictionary<string, string>? sources )
        => this._unhandledSources.TryRemove( projectId, out sources );

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