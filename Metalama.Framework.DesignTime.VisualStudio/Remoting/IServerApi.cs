// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.VisualStudio.Remoting;

internal interface IServerApi
{
    // The client should not await this call, otherwise we will have a deadlock.
    Task HelloAsync( string projectId, CancellationToken cancellationToken = default );

    Task<string> PreviewAsync( string fileName, CancellationToken cancellationToken = default );
}

internal interface IClientApi
{
    Task PublishGeneratedCodeAsync( string projectId, ImmutableDictionary<string, string> sources, CancellationToken cancellationToken = default );
}