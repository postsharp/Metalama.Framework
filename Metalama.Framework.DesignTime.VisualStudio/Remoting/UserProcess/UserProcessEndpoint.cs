// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.CodeFixes;
using Metalama.Framework.DesignTime.Contracts.Diagnostics;
using Metalama.Framework.DesignTime.Diagnostics;
using Metalama.Framework.DesignTime.Rpc;
using Metalama.Framework.DesignTime.VisualStudio.Remoting.Api;
using Metalama.Framework.Engine.DesignTime.CodeFixes;
using Metalama.Framework.Engine.Services;
using Microsoft.CodeAnalysis.Text;
using StreamJsonRpc;
using System.Collections.Concurrent;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.VisualStudio.Remoting.UserProcess;

/// <summary>
/// Implements the remoting API of the user process.
/// </summary>
internal sealed partial class UserProcessEndpoint : ClientEndpoint<IAnalysisProcessApi>, ICodeRefactoringDiscoveryService, ICodeActionExecutionService
{
    private readonly ApiImplementation _apiImplementation;
    private readonly ConcurrentDictionary<ProjectKey, ImmutableDictionary<string, string>> _cachedGeneratedSources = new();
    private readonly ConcurrentDictionary<ProjectKey, IProjectHandlerCallbackApi> _projectHandlers = new();

    private ImmutableDictionary<ProjectKey, ImmutableArray<IDiagnosticData>> _compileTimeErrorsPerProject =
        ImmutableDictionary<ProjectKey, ImmutableArray<IDiagnosticData>>.Empty;

    public UserProcessEndpoint( GlobalServiceProvider serviceProvider, string pipeName ) : base(
        serviceProvider.Underlying,
        pipeName,
        JsonSerializationBinderFactory.Instance )
    {
        this._apiImplementation = new ApiImplementation( this );
    }

    protected override void ConfigureRpc( JsonRpc rpc )
    {
        rpc.AddLocalRpcTarget<IUserProcessApi>( this._apiImplementation, null );
        rpc.AddLocalRpcTarget<IProjectHandlerCallbackApi>( this._apiImplementation, null );
    }

    public async Task RegisterProjectCallbackAsync( ProjectKey projectKey, IProjectHandlerCallbackApi callback, CancellationToken cancellationToken = default )
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

    public event Action<ImmutableDictionary<ProjectKey, ImmutableArray<IDiagnosticData>>>? CompileTimeErrorsChanged;

    public event Action<ProjectKey>? AspectClassesChanged;

    public event Action<ProjectKey>? AspectInstancesChanged;

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

    // ReSharper disable once UnusedParameter.Global
    public Task<ImmutableDictionary<ProjectKey, ImmutableArray<IDiagnosticData>>> GetCompileTimeErrorsAsync( CancellationToken cancellationToken )
        => Task.FromResult( this._compileTimeErrorsPerProject );

    private void SetCompileTimeErrors( ProjectKey projectKey, IReadOnlyCollection<DiagnosticData> diagnostics )
    {
        while ( true )
        {
            var oldDictionary = this._compileTimeErrorsPerProject;
            var dictionary = oldDictionary.SetItem( projectKey, diagnostics.ToImmutableArray<IDiagnosticData>() );

            if ( Interlocked.CompareExchange( ref this._compileTimeErrorsPerProject, dictionary, oldDictionary ) == oldDictionary )
            {
                break;
            }
        }

        this.CompileTimeErrorsChanged?.Invoke( this._compileTimeErrorsPerProject );
    }
}