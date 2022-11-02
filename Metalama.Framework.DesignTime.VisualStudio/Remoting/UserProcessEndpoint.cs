// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.CodeFixes;
using Metalama.Framework.Engine.CodeFixes;
using Microsoft.CodeAnalysis.Text;
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
    private readonly ConcurrentDictionary<ProjectKey, ImmutableDictionary<string, string>> _cachedGeneratedSources = new();
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

    public async Task RegisterProjectCallbackAsync( ProjectKey projectKey, IProjectHandlerCallback callback, CancellationToken cancellationToken = default )
    {
        await this.WaitUntilInitializedAsync( nameof(this.RegisterProjectCallbackAsync), cancellationToken );
        this._projectHandlers[projectKey] = callback;

        await (await this.GetServerApiAsync( nameof(this.RegisterProjectCallbackAsync), cancellationToken )).RegisterProjectCallbackAsync(
            projectKey,
            cancellationToken );
    }

    public bool TryGetCachedGeneratedSources( ProjectKey projectKey, out ImmutableDictionary<string, string>? sources )
    {
        if ( this._cachedGeneratedSources.TryGetValue( projectKey, out sources ) )
        {
            this.Logger.Trace?.Log( $"Found cached generated sources for '{projectKey}'." );

            return true;
        }
        else
        {
            this.Logger.Trace?.Log( $"No cached generated sources for '{projectKey}'." );

            return false;
        }
    }

    public event Action<bool>? IsEditingCompileTimeCodeChanged;

    async Task<ComputeRefactoringResult> ICodeRefactoringDiscoveryService.ComputeRefactoringsAsync(
        ProjectKey projectKey,
        string syntaxTreePath,
        TextSpan span,
        CancellationToken cancellationToken )
    {
        var peer = await this.GetServerApiAsync( nameof(ICodeRefactoringDiscoveryService.ComputeRefactoringsAsync), cancellationToken );

        return await peer.ComputeRefactoringsAsync(
            projectKey,
            syntaxTreePath,
            span,
            cancellationToken );
    }

    async Task<CodeActionResult> ICodeActionExecutionService.ExecuteCodeActionAsync(
        ProjectKey projectKey,
        CodeActionModel codeActionModel,
        bool isComputingPreview,
        CancellationToken cancellationToken )
    {
        var peer = await this.GetServerApiAsync( nameof(ICodeActionExecutionService.ExecuteCodeActionAsync), cancellationToken );

        return await peer.ExecuteCodeActionAsync( projectKey, codeActionModel, isComputingPreview, cancellationToken );
    }
}