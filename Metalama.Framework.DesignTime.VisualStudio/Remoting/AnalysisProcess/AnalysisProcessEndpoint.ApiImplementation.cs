// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.DesignTime.CodeFixes;
using Metalama.Framework.DesignTime.CodeLens;
using Metalama.Framework.DesignTime.Contracts.CodeLens;
using Metalama.Framework.DesignTime.Preview;
using Metalama.Framework.DesignTime.Rpc;
using Metalama.Framework.DesignTime.Utilities;
using Metalama.Framework.DesignTime.VisualStudio.Remoting.Api;
using Metalama.Framework.Engine.CodeFixes;
using Metalama.Framework.Engine.Utilities.Threading;
using Microsoft.CodeAnalysis.Text;

namespace Metalama.Framework.DesignTime.VisualStudio.Remoting.AnalysisProcess;

internal partial class AnalysisProcessEndpoint
{
    /// <summary>
    /// Implementation of the <see cref="IAnalysisProcessApi"/> interface. Processes remote requests.
    /// </summary>
    private class ApiImplementation : IAnalysisProcessApi
    {
        private readonly AnalysisProcessEndpoint _parent;
        private readonly IUserProcessApi _client;

        public ApiImplementation( AnalysisProcessEndpoint parent, IUserProcessApi client )
        {
            this._parent = parent;
            this._client = client;
        }

        public async Task RegisterProjectCallbackAsync( ProjectKey projectKey, CancellationToken cancellationToken )
        {
            this._parent.Logger.Trace?.Log( $"The client '{projectKey}' has connected. Registering the callback communication." );

            this._parent._connectedProjectCallbacks[projectKey] = projectKey;

            Thread.MemoryBarrier();

            // If we received source before the client connected, publish it for the client now.
            if ( this._parent._generatedSourcesForUnconnectedClients.TryRemove( projectKey, out var sources ) )
            {
                this._parent.Logger.Trace?.Log( $"Publishing source for the client '{projectKey}'." );

                await this._client.PublishGeneratedCodeAsync( projectKey, sources, cancellationToken );
            }

            this._parent.ClientConnected?.Invoke( this._parent, new ClientConnectedEventArgs( projectKey ) );
        }

        public Task<PreviewTransformationResult> PreviewTransformationAsync( ProjectKey projectKey, string syntaxTreeName, CancellationToken cancellationToken )
        {
            var implementation = this._parent._serviceProvider.GetRequiredService<ITransformationPreviewServiceImpl>();

            return implementation.PreviewTransformationAsync( projectKey, syntaxTreeName, cancellationToken );
        }

        public Task OnCompileTimeCodeEditingCompletedAsync( CancellationToken cancellationToken = default )
        {
            this._parent._eventHub.OnCompileTimeCodeCompletedEditing();

            return Task.CompletedTask;
        }

        public Task OnUserInterfaceAttachedAsync( CancellationToken cancellationToken = default )
        {
            this._parent._eventHub.OnUserInterfaceAttached();

            return Task.CompletedTask;
        }

        public Task<CodeLensSummary> GetCodeLensSummaryAsync(
            ProjectKey projectKey,
            SerializableDeclarationId symbolId,
            CancellationToken cancellationToken = default )
        {
            var implementation = this._parent._serviceProvider.GetService<ICodeLensServiceImpl>();

            if ( implementation == null )
            {
                return Task.FromResult( CodeLensSummary.NotAvailable );
            }

            return implementation.GetCodeLensSummaryAsync( projectKey, symbolId, cancellationToken );
        }

        public Task<ICodeLensDetailsTable> GetCodeLensDetailsAsync(
            ProjectKey projectKey,
            SerializableDeclarationId symbolId,
            CancellationToken cancellationToken = default )
        {
            var implementation = this._parent._serviceProvider.GetService<ICodeLensServiceImpl>();

            if ( implementation == null )
            {
                return Task.FromResult<ICodeLensDetailsTable>( CodeLensDetailsTable.Empty );
            }

            return implementation.GetCodeLensDetailsAsync( projectKey, symbolId, cancellationToken );
        }

        public Task<ComputeRefactoringResult> ComputeRefactoringsAsync(
            ProjectKey projectKey,
            string syntaxTreePath,
            TextSpan span,
            CancellationToken cancellationToken )
        {
            var service = this._parent._serviceProvider.GetRequiredService<CodeRefactoringDiscoveryService>();

            return service.ComputeRefactoringsAsync( projectKey, syntaxTreePath, span, cancellationToken.IgnoreIfDebugging().ToTestable() );
        }

        public Task<CodeActionResult> ExecuteCodeActionAsync(
            ProjectKey projectKey,
            CodeActionModel codeActionModel,
            bool isComputingPreview,
            CancellationToken cancellationToken )
        {
            var service = this._parent._serviceProvider.GetRequiredService<CodeActionExecutionService>();

            return service.ExecuteCodeActionAsync(
                projectKey,
                codeActionModel,
                isComputingPreview,
                cancellationToken.ToTestable() );
        }
    }
}