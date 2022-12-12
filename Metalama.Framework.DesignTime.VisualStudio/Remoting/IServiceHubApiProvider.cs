﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Services;

namespace Metalama.Framework.DesignTime.VisualStudio.Remoting;

internal interface IServiceHubApiProvider : IGlobalService
{
    ValueTask<IServiceHubApi> GetApiAsync( string callerName, CancellationToken cancellationToken );
}