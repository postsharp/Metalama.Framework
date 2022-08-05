// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.DesignTime.CodeFixes;
using Metalama.Framework.DesignTime.Contracts;
using Metalama.Framework.DesignTime.Pipeline;
using Metalama.Framework.DesignTime.Preview;
using Metalama.Framework.Engine.CodeFixes;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis.Text;

namespace Metalama.Framework.DesignTime.VisualStudio.Remoting;

internal partial class AnalysisProcessEndpoint
{
    /// <summary>
    /// Implementation of the <see cref="IAnalysisProcessApi"/> interface. Processes remote requests.
    /// </summary>
    private class ApiImplementation : IAnalysisProcessApi
    {
        private readonly AnalysisProcessEndpoint _parent;

        public ApiImplementation( AnalysisProcessEndpoint parent )
        {
            this._parent = parent;
        }

        public async Task OnProjectHandlerReadyAsync( ProjectKey projectKey, CancellationToken cancellationToken )
        {
            this._parent._logger.Trace?.Log( $"The client '{projectKey}' has connected." );

            this._parent._connectedClients[projectKey] = projectKey;

            Thread.MemoryBarrier();

            // If we received source before the client connected, publish it for the client now.
            if ( this._parent._sourcesForUnconnectedClients.TryRemove( projectKey, out var sources ) )
            {
                this._parent._logger.Trace?.Log( $"Publishing source for the client '{projectKey}'." );

                await this._parent._client!.PublishGeneratedCodeAsync( projectKey, sources, cancellationToken );
            }

            this._parent.ClientConnected?.Invoke( this._parent, new ClientConnectedEventArgs( projectKey ) );
        }

        public Task<PreviewTransformationResult> PreviewTransformationAsync( ProjectKey projectKey, string syntaxTreeName, CancellationToken cancellationToken )
        {
            var implementation = this._parent._serviceProvider.GetRequiredService<ITransformationPreviewServiceImpl>();

            return implementation.PreviewTransformationAsync( projectKey, syntaxTreeName, cancellationToken );
        }

        public async Task OnCompileTimeCodeEditingCompletedAsync( CancellationToken cancellationToken = default )
        {
            var service = this._parent._serviceProvider.GetRequiredService<ICompileTimeCodeEditingStatusService>();
            await service.OnEditingCompileTimeCodeCompletedAsync();
        }

        public Task OnUserInterfaceAttachedAsync( CancellationToken cancellationToken = default )
        {
            var implementation = this._parent._serviceProvider.GetService<ICompileTimeCodeEditingStatusService>();

            implementation?.OnUserInterfaceAttached();

            return Task.CompletedTask;
        }

        public Task<ComputeRefactoringResult> ComputeRefactoringsAsync(
            ProjectKey projectKey,
            string syntaxTreePath,
            TextSpan span,
            CancellationToken cancellationToken )
        {
            var service = this._parent._serviceProvider.GetRequiredService<CodeRefactoringDiscoveryService>();

            return service.ComputeRefactoringsAsync( projectKey, syntaxTreePath, span, cancellationToken );
        }

        public Task<CodeActionResult> ExecuteCodeActionAsync( ProjectKey projectKey, CodeActionModel codeActionModel, CancellationToken cancellationToken )
        {
            var service = this._parent._serviceProvider.GetRequiredService<CodeActionExecutionService>();

            return service.ExecuteCodeActionAsync( projectKey, codeActionModel, cancellationToken );
        }
    }
}