// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Metalama.Framework.Code;
using Metalama.Framework.DesignTime.AspectExplorer;
using Metalama.Framework.DesignTime.CodeFixes;
using Metalama.Framework.DesignTime.CodeLens;
using Metalama.Framework.DesignTime.Contracts.CodeLens;
using Metalama.Framework.DesignTime.Preview;
using Metalama.Framework.DesignTime.Rpc;

namespace Metalama.Framework.DesignTime.VisualStudio.Remoting.Api;

/// <summary>
/// Defines the remote API implemented by the analysis process.
/// </summary>
internal interface IAnalysisProcessApi : ICodeRefactoringDiscoveryService, ICodeActionExecutionService
{
    /// <summary>
    /// Notifies the analysis process that the user process is now ready to process notifications for a given project, which means that the analysis process will start
    /// calling <see cref="IProjectHandlerCallbackApi.PublishGeneratedCodeAsync"/> for this project.
    /// </summary>
    Task RegisterProjectCallbackAsync( ProjectKey projectKey, [UsedImplicitly] CancellationToken cancellationToken = default );

    /// <summary>
    /// Computes the transformed code by running the pipeline, and returns the result.
    /// </summary>
    Task<SerializablePreviewTransformationResult> PreviewTransformationAsync(
        ProjectKey projectKey,
        string syntaxTreeName,
        IEnumerable<string>? additionalSyntaxTreeNames = null,
        CancellationToken cancellationToken = default );

    /// <summary>
    /// Notifies that the user is done editing compile-time code, so the pipeline can be resumed.
    /// </summary>
    Task OnCompileTimeCodeEditingCompletedAsync( [UsedImplicitly] CancellationToken cancellationToken = default );

    /// <summary>
    /// Notifies that a user interface (not only the user process, but our VSX) is attached to the user-process services and
    /// listens to <see cref="IUserProcessApi.OnIsEditingCompileTimeCodeChanged"/>, so that the pipeline does not report
    /// editing-in-progress situations as errors.
    /// </summary>
    Task OnUserInterfaceAttachedAsync( [UsedImplicitly] CancellationToken cancellationToken = default );

    /// <summary>
    /// Gets the inline summary code lens text for a symbol.
    /// </summary>
    Task<CodeLensSummary> GetCodeLensSummaryAsync(
        ProjectKey projectKey,
        SerializableDeclarationId symbolId,
        [UsedImplicitly] CancellationToken cancellationToken = default );

    /// <summary>
    /// Gets the detailed code lens content that appears when the user clicks on the summary text.
    /// </summary>
    Task<ICodeLensDetailsTable> GetCodeLensDetailsAsync(
        ProjectKey projectKey,
        SerializableDeclarationId symbolId,
        [UsedImplicitly] CancellationToken cancellationToken = default );

    /// <summary>
    /// Gets the aspect classes in a project, represented using the string form of <see cref="SerializableTypeId"/>.
    /// </summary>
    Task<IEnumerable<string>> GetAspectClassesAsync( ProjectKey projectKey, [UsedImplicitly] CancellationToken cancellationToken = default );

    /// <summary>
    /// Gets the aspect instances for an aspect class in a project.
    /// <paramref name="aspectClassId"/> is the string form of <see cref="SerializableTypeId"/>.
    /// </summary>
    Task<IEnumerable<AspectDatabaseAspectInstance>> GetAspectInstancesAsync(
        ProjectKey projectKey,
        string aspectClassAssembly,
        string aspectClassId,
        [UsedImplicitly] CancellationToken cancellationToken = default );
}