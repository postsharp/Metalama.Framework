// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.Rpc;
using Metalama.Framework.DesignTime.SourceGeneration;
using Metalama.Framework.DesignTime.VisualStudio.Remoting.Api;
using Metalama.Framework.DesignTime.VisualStudio.Remoting.UserProcess;
using Metalama.Framework.Engine;
using Metalama.Framework.Engine.Options;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities.Threading;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.VisualStudio;

/// <summary>
/// Implementation of <see cref="ProjectHandler"/> in the Visual Studio user process. It receives generated source code
/// from the analysis process.
/// </summary>
internal class VsUserProcessProjectHandler : ProjectHandler, IProjectHandlerCallbackApi
{
    private readonly UserProcessServiceHubEndpoint _userProcessEndpoint;
    private readonly IProjectHandlerObserver? _observer;
    private ImmutableDictionary<string, string>? _sources;

    public VsUserProcessProjectHandler( GlobalServiceProvider serviceProvider, IProjectOptions projectOptions, ProjectKey projectKey ) : base(
        serviceProvider,
        projectOptions,
        projectKey )
    {
        this._userProcessEndpoint = serviceProvider.GetRequiredService<UserProcessServiceHubEndpoint>();
        this._observer = serviceProvider.GetService<IProjectHandlerObserver>();

        this.PendingTasks.Run( () => this._userProcessEndpoint.RegisterProjectCallbackAsync( this.ProjectKey, this ) );
    }

    public override SourceGeneratorResult GenerateSources( Compilation compilation, TestableCancellationToken cancellationToken )
    {
        if ( this._sources == null )
        {
            // If we have not received the source yet, see if it was received by the client before we were created.
            if ( this._userProcessEndpoint.TryGetGenerateSourcesIfAvailable( this.ProjectKey, out var sources ) )
            {
                this.Logger.Trace?.Log( $"Generated sources for '{this.ProjectKey}' were retrieved from ServiceClient." );
                this._sources = sources;
            }
            else
            {
                this.Logger.Warning?.Log( $"Information about generated sources for '{this.ProjectKey}' is not available." );

                return SourceGeneratorResult.Empty;
            }
        }

        return new TextSourceGeneratorResult( this._sources.AssertNotNull() );
    }

    Task IProjectHandlerCallbackApi.PublishGeneratedCodeAsync(
        ProjectKey projectKey,
        ImmutableDictionary<string, string> sources,
        CancellationToken cancellationToken )
    {
        this._sources = sources;
        this._observer?.OnGeneratedCodePublished( sources );

        return Task.CompletedTask;
    }
}