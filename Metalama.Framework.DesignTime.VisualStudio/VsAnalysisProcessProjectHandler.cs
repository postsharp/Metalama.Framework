// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.Rpc;
using Metalama.Framework.DesignTime.VisualStudio.Remoting.AnalysisProcess;
using Metalama.Framework.Engine.Options;
using Metalama.Framework.Engine.Services;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.VisualStudio;

/// <summary>
/// Implementation of <see cref="ProjectHandler"/> in the Visual Studio analysis process. It establishes a connection to the user process
/// to publish generated sources.
/// </summary>
internal sealed class VsAnalysisProcessProjectHandler : AnalysisProcessProjectHandler
{
    private readonly AnalysisProcessEndpoint? _endpoint;

    public VsAnalysisProcessProjectHandler( GlobalServiceProvider serviceProvider, IProjectOptions projectOptions, ProjectKey projectKey ) : base(
        serviceProvider,
        projectOptions,
        projectKey )
    {
        this._endpoint = serviceProvider.GetService<AnalysisProcessEndpoint>();

        if ( this._endpoint != null )
        {
            this._endpoint.ClientConnected += this.OnClientConnected;
            this.PendingTasks.Run( () => this._endpoint.RegisterProjectAsync( this.ProjectKey ) );
        }
        else
        {
            this.Logger.Warning?.Log( "The project handler was created without an endpoint." );
        }
    }

    private void OnClientConnected( object? sender, ClientConnectedEventArgs e )
    {
        if ( e.ProjectKey == this.ProjectKey )
        {
            // When a client connects, we update the touch file to force that client to request the info again.
            this.UpdateTouchFile();
        }
    }

    protected override Task PublishGeneratedSourcesAsync( ProjectKey projectKey, CancellationToken cancellationToken )
    {
        if ( this._endpoint == null )
        {
            this.Logger.Warning?.Log( $"Do not publish the generated source for '{projectKey}' because there is no endpoint." );

            return Task.CompletedTask;
        }

        if ( this.LastSourceGeneratorResult == null )
        {
            this.Logger.Warning?.Log( $"Do not publish the generated source for '{projectKey}' because there is none." );

            return Task.CompletedTask;
        }
        else
        {
            this.Logger.Trace?.Log( $"Publishing generated source of '{projectKey}' to the user process." );

            var generatedSources = this.LastSourceGeneratorResult.AdditionalSources
                .ToImmutableDictionary( x => x.Key, x => x.Value.GeneratedSyntaxTree.ToString() );

            return this._endpoint.PublishGeneratedSourcesAsync(
                projectKey,
                generatedSources,
                cancellationToken );
        }
    }
}