// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.DesignTime.CodeFixes;
using Metalama.Framework.DesignTime.CodeLens;
using Metalama.Framework.DesignTime.Contracts.CodeLens;
using Metalama.Framework.DesignTime.Rpc;

namespace Metalama.Framework.DesignTime.VisualStudio.Remoting.Api;

/// <summary>
/// Defines the remote API implemented by the analysis process.
/// </summary>
internal interface IAnalysisProcessApi : IRpcApi, ICodeRefactoringDiscoveryService, ICodeActionExecutionService
{
    /// <summary>
    /// Notifies the analysis process that the user process is now ready to process notifications for a given project, which means that the analysis process will start
    /// calling <see cref="IProjectHandlerCallbackApi.PublishGeneratedCodeAsync"/> for this project.
    /// </summary>
    Task RegisterProjectCallbackAsync( ProjectKey projectKey, CancellationToken cancellationToken = default );

    /// <summary>
    /// Computes the transformed code by running the pipeline, and returns the result.
    /// </summary>
    Task<SerializablePreviewTransformationResult> PreviewTransformationAsync(
        ProjectKey projectKey,
        string syntaxTreeName,
        CancellationToken cancellationToken );

    /// <summary>
    /// Notifies that the user is done editing compile-time code, so the pipeline can be resumed.
    /// </summary>
    Task OnCompileTimeCodeEditingCompletedAsync( CancellationToken cancellationToken = default );

    /// <summary>
    /// Notifies that a user interface (not only the user process, but our VSX) is attached to the user-process services and
    /// listens to <see cref="IUserProcessApi.OnIsEditingCompileTimeCodeChanged"/>, so that the pipeline does not report
    /// editing-in-progress situations as errors.
    /// </summary>
    Task OnUserInterfaceAttachedAsync( CancellationToken cancellationToken = default );

    /// <summary>
    /// Gets the inline summary code lens text for a symbol.
    /// </summary>
    Task<CodeLensSummary> GetCodeLensSummaryAsync( ProjectKey projectKey, SerializableDeclarationId symbolId, CancellationToken cancellationToken = default );

    /// <summary>
    /// Gets the detailed code lens content that appears when the user clicks on the summary text.
    /// </summary>
    Task<ICodeLensDetailsTable> GetCodeLensDetailsAsync(
        ProjectKey projectKey,
        SerializableDeclarationId symbolId,
        CancellationToken cancellationToken = default );
}