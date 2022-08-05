// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.DesignTime.CodeFixes;
using Metalama.Framework.DesignTime.Contracts;

namespace Metalama.Framework.DesignTime.VisualStudio.Remoting;

/// <summary>
/// Defines the remote API implemented by the analysis process.
/// </summary>
internal interface IAnalysisProcessApi : ICodeRefactoringDiscoveryService, ICodeActionExecutionService
{
    /// <summary>
    /// Notifies the analysis process that the user process is now ready to process notifications for a given project, which means that the analysis process will start
    /// calling <see cref="IProjectHandlerCallback.PublishGeneratedCodeAsync"/> for this project.
    /// </summary>
    Task OnProjectHandlerReadyAsync( ProjectKey projectKey, CancellationToken cancellationToken = default );

    /// <summary>
    /// Computes the transformed code by running the pipeline, and returns the result.
    /// </summary>
    Task<PreviewTransformationResult> PreviewTransformationAsync(
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
}