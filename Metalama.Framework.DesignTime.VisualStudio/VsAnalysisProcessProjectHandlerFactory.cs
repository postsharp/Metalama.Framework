// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.Rpc;
using Metalama.Framework.Engine.Options;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Services;
using System.Collections.Concurrent;

namespace Metalama.Framework.DesignTime.VisualStudio;

internal sealed class VsAnalysisProcessProjectHandlerFactory : IGlobalService
{
    private readonly GlobalServiceProvider _serviceProvider;
    private readonly ConcurrentDictionary<ProjectKey, Lazy<VsAnalysisProcessProjectHandler>> _handlers = new();

    public VsAnalysisProcessProjectHandlerFactory( GlobalServiceProvider serviceProvider )
    {
        this._serviceProvider = serviceProvider;
    }

    public VsAnalysisProcessProjectHandler GetOrCreateProjectHandler( IProjectOptions projectOptions, ProjectKey projectKey )
        => this._handlers.GetOrAdd(
                projectKey,
                new Lazy<VsAnalysisProcessProjectHandler>( () => new VsAnalysisProcessProjectHandler( this._serviceProvider, projectOptions, projectKey ) ) )
            .Value;
}