// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.VisualStudio.Remoting;

/// <summary>
/// A user-process interface invoked by the analysis process to publish generated code.
/// </summary>
internal interface IProjectHandlerCallback
{
    /// <summary>
    /// Publishes generated code to the user process.
    /// </summary>
    Task PublishGeneratedCodeAsync( ProjectKey projectKey, ImmutableDictionary<string, string> sources, CancellationToken cancellationToken = default );
}