// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Project;

namespace Metalama.Framework.DesignTime.VisualStudio.Remoting;

internal interface IServiceHubApi
{
    Task RegisterEndpointAsync( string pipeName, CancellationToken cancellationToken );

    Task RegisterProjectAsync( string projectId, string pipeName, CancellationToken cancellationToken );

    Task UnregisterEndpointAsync( string pipeName, CancellationToken cancellationToken );

    Task UnregisterProjectAsync( string projectId, CancellationToken cancellationToken );
}

internal interface IServiceHubApiProvider : IService
{
    ValueTask<IServiceHubApi> GetApiAsync( CancellationToken cancellationToken );
}