// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

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
    Task PublishGeneratedCodeAsync( string projectId, ImmutableDictionary<string, string> sources, CancellationToken cancellationToken = default );
}