// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.DesignTime.CodeFixes;
using Metalama.Framework.DesignTime.Contracts;

namespace Metalama.Framework.DesignTime.VisualStudio.Remoting;

internal interface IServerApi : ICodeActionDiscoveryService, ICodeActionExecutionService
{
    // The client should not await this call, otherwise we will have a deadlock.
    Task RegisterProjectHandlerAsync( string projectId, CancellationToken cancellationToken = default );

    Task<PreviewTransformationResult> PreviewTransformationAsync(
        string projectId,
        string syntaxTreeName,
        CancellationToken cancellationToken );

    Task OnCompileTimeCodeEditingCompletedAsync( CancellationToken cancellationToken = default );
}