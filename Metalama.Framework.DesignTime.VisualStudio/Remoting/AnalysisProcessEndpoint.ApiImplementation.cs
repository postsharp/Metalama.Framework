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

        public async Task OnUserProcessProjectHandlerConnectedAsync( string projectId, CancellationToken cancellationToken )
        {
            this._parent.Logger.Trace?.Log( $"The user process for project '{projectId}' has connected." );

            this._parent._connectedClients[projectId] = projectId;

            Thread.MemoryBarrier();

            // If we received source before the client connected, publish it for the client now.
            if ( this._parent._sourcesForUnconnectedClients.TryRemove( projectId, out var sources ) )
            {
                this._parent.Logger.Trace?.Log( $"Publishing source for the project '{projectId}'." );

                await this._parent._client!.PublishGeneratedCodeAsync( projectId, sources, cancellationToken );
            }

            this._parent.ClientConnected?.Invoke( this._parent, new ClientConnectedEventArgs( projectId ) );
        }

        public Task<PreviewTransformationResult> PreviewTransformationAsync( string projectId, string syntaxTreeName, CancellationToken cancellationToken )
        {
            var implementation = this._parent._serviceProvider.GetRequiredService<ITransformationPreviewServiceImpl>();

            return implementation.PreviewTransformationAsync( projectId, syntaxTreeName, cancellationToken );
        }

        public Task OnCompileTimeCodeEditingCompletedAsync( CancellationToken cancellationToken = default )
        {
            var service = this._parent._serviceProvider.GetRequiredService<ICompileTimeCodeEditingStatusService>();
            service.OnEditingCompileTimeCodeCompleted();

            return Task.CompletedTask;
        }

        public Task OnUserInterfaceAttachedAsync( CancellationToken cancellationToken = default )
        {
            var implementation = this._parent._serviceProvider.GetService<ICompileTimeCodeEditingStatusService>();

            implementation?.OnUserInterfaceAttached();

            return Task.CompletedTask;
        }

        public Task<ComputeRefactoringResult> ComputeRefactoringsAsync(
            string projectId,
            string syntaxTreePath,
            TextSpan span,
            CancellationToken cancellationToken )
        {
            var service = this._parent._serviceProvider.GetRequiredService<CodeRefactoringDiscoveryService>();

            return service.ComputeRefactoringsAsync( projectId, syntaxTreePath, span, cancellationToken );
        }

        public Task<CodeActionResult> ExecuteCodeActionAsync( string projectId, CodeActionModel codeActionModel, CancellationToken cancellationToken )
        {
            var service = this._parent._serviceProvider.GetRequiredService<CodeActionExecutionService>();

            return service.ExecuteCodeActionAsync( projectId, codeActionModel, cancellationToken );
        }
    }
}