// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Metalama.Framework.DesignTime.Rpc;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.VisualStudio.Remoting.Api;

/// <summary>
/// A user-process interface invoked by the analysis process to publish generated code.
/// </summary>
internal interface IProjectHandlerCallbackApi : IRpcApi
{
    /// <summary>
    /// Publishes generated code to the user process.
    /// </summary>
    Task PublishGeneratedCodeAsync(
        ProjectKey projectKey,
        ImmutableDictionary<string, string> sources,
        [UsedImplicitly] CancellationToken cancellationToken = default );
}