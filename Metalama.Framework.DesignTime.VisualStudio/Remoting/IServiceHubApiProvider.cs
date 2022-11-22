// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.VisualStudio.Remoting.Api;
using Metalama.Framework.Project;

namespace Metalama.Framework.DesignTime.VisualStudio.Remoting;

internal interface IServiceHubApiProvider : IService
{
    ValueTask<IServiceHubApi> GetApiAsync( string callerName, CancellationToken cancellationToken );
}