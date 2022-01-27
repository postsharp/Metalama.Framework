// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace Metalama.Framework.Engine.DesignTime.Remoting;

internal interface IServerApi
{
    Task<string> PreviewAsync( string fileName, CancellationToken cancellationToken = default );
}

internal interface IClientApi
{
    Task PublishGeneratedCodeAsync( string projectId, ImmutableDictionary<string, string> sources, CancellationToken cancellationToken = default );
}