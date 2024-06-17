// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.DesignTime.AspectExplorer;
using Metalama.Framework.DesignTime.CodeFixes;
using Metalama.Framework.DesignTime.CodeLens;
using Metalama.Framework.DesignTime.Contracts.CodeLens;
using Metalama.Framework.DesignTime.Preview;
using Metalama.Framework.DesignTime.Rpc;
using Metalama.Framework.DesignTime.Utilities;
using Metalama.Framework.DesignTime.VisualStudio.Remoting.Api;
using Metalama.Framework.Engine.DesignTime.CodeFixes;
using Metalama.Framework.Engine.Utilities.Threading;
using Microsoft.CodeAnalysis.Text;

namespace Metalama.Framework.DesignTime.VisualStudio.Remoting.AnalysisProcess;

internal sealed partial class AnalysisProcessEndpoint
{
    /// <summary>
    /// Implementation of the <see cref="IAnalysisProcessApi"/> interface. Processes remote requests.
    /// </summary>
    private sealed class ApiImplementation : IAnalysisProcessApi
    {
        private readonly AnalysisProcessEndpoint _parent;
        private readonly IUserProcessApi _client;
        private readonly ICodeLensServiceImpl? _codeLensServiceImpl;
        private readonly CodeRefactoringDiscoveryService? _codeRefactoringDiscoveryService;
        private readonly CodeActionExecutionService? _codeActionExecutionService;
        private readonly AspectDatabase? _aspectDatabase;

        public ApiImplementation( AnalysisProcessEndpoint parent, IUserProcessApi client )
        {
            this._parent = parent;
            this._client = client;

            var serviceProvider = this._parent._serviceProvider;
            this._codeLensServiceImpl = serviceProvider.GetService<ICodeLensServiceImpl>();
            this._codeRefactoringDiscoveryService = serviceProvider.GetService<CodeRefactoringDiscoveryService>();
            this._codeActionExecutionService = serviceProvider.GetService<CodeActionExecutionService>();
            this._aspectDatabase = serviceProvider.GetService<AspectDatabase>();
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

        public Task<SerializablePreviewTransformationResult> PreviewTransformationAsync(
            ProjectKey projectKey,
            string syntaxTreeName,
            CancellationToken cancellationToken )
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
            if ( this._codeLensServiceImpl == null )
            {
                this._parent.Logger.Warning?.Log( "The CodeLensService is not registered." );

                return Task.FromResult( CodeLensSummary.NotAvailable );
            }

            return this._codeLensServiceImpl.GetCodeLensSummaryAsync( projectKey, symbolId, cancellationToken.ToTestable() );
        }

        public Task<ICodeLensDetailsTable> GetCodeLensDetailsAsync(
            ProjectKey projectKey,
            SerializableDeclarationId symbolId,
            CancellationToken cancellationToken = default )
        {
            if ( this._codeLensServiceImpl == null )
            {
                this._parent.Logger.Warning?.Log( "The CodeLensService is not registered." );

                return Task.FromResult<ICodeLensDetailsTable>( CodeLensDetailsTable.Empty );
            }

            return this._codeLensServiceImpl.GetCodeLensDetailsAsync( projectKey, symbolId, cancellationToken.ToTestable() );
        }

        public Task<ComputeRefactoringResult> ComputeRefactoringsAsync(
            ProjectKey projectKey,
            string syntaxTreePath,
            TextSpan span,
            CancellationToken cancellationToken )
        {
            if ( this._codeRefactoringDiscoveryService == null )
            {
                this._parent.Logger.Warning?.Log( "The CodeRefactoringDiscoveryService is not registered." );

                return Task.FromResult( ComputeRefactoringResult.Empty );
            }

            return this._codeRefactoringDiscoveryService.ComputeRefactoringsAsync(
                projectKey,
                syntaxTreePath,
                span,
                cancellationToken.IgnoreIfDebugging().ToTestable() );
        }

        public Task<CodeActionResult> ExecuteCodeActionAsync(
            ProjectKey projectKey,
            CodeActionModel codeActionModel,
            bool isComputingPreview,
            CancellationToken cancellationToken )
        {
            if ( this._codeActionExecutionService == null )
            {
                throw new InvalidOperationException();
            }

            return this._codeActionExecutionService.ExecuteCodeActionAsync(
                projectKey,
                codeActionModel,
                isComputingPreview,
                cancellationToken );
        }

        public Task<IEnumerable<string>> GetAspectClassesAsync( ProjectKey projectKey, CancellationToken cancellationToken )
        {
            if ( this._aspectDatabase is null )
            {
                this._parent.Logger.Warning?.Log( "The AspectDatabase is not registered." );

                return Task.FromResult( Enumerable.Empty<string>() );
            }

            return this._aspectDatabase.GetAspectClassesAsync( projectKey, cancellationToken );
        }

        public Task<IEnumerable<AspectDatabaseAspectInstance>> GetAspectInstancesAsync(
            ProjectKey projectKey,
            string aspectClassAssembly,
            string aspectClassId,
            CancellationToken cancellationToken )
        {
            if ( this._aspectDatabase is null )
            {
                this._parent.Logger.Warning?.Log( "The AspectDatabase is not registered." );

                return Task.FromResult( Enumerable.Empty<AspectDatabaseAspectInstance>() );
            }

            return this._aspectDatabase.GetAspectInstancesAsync( projectKey, aspectClassAssembly, new SerializableTypeId( aspectClassId ), cancellationToken );
        }
    }
}