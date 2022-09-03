// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.DesignTime.VisualStudio.Remoting;
using Metalama.Framework.Engine.Options;
using Metalama.Framework.Project;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.VisualStudio;

/// <summary>
/// Implementation of <see cref="ProjectHandler"/> in the Visual Studio analysis process. It establishes a connection to the user process
/// to publish generated sources.
/// </summary>
internal class VsAnalysisProcessProjectHandler : AnalysisProcessProjectHandler
{
    private readonly AnalysisProcessEndpoint? _serviceHost;

    public VsAnalysisProcessProjectHandler( IServiceProvider serviceProvider, IProjectOptions projectOptions ) : base( serviceProvider, projectOptions )
    {
        this._serviceHost = serviceProvider.GetService<AnalysisProcessEndpoint>();

        if ( this._serviceHost != null )
        {
            this._serviceHost.ClientConnected += this.OnClientConnected;
            this._serviceHost.RegisterProject( projectOptions.ProjectId );
        }
    }

    private void OnClientConnected( object? sender, ClientConnectedEventArgs e )
    {
        if ( e.ProjectId == this.ProjectOptions.ProjectId )
        {
            // When a client connects, we update the touch file to force that client to request the info again.
            this.UpdateTouchFile();
        }
    }

    protected override Task PublishGeneratedSourcesAsync( string projectId, CancellationToken cancellationToken )
    {
        if ( this._serviceHost != null && this.LastSourceGeneratorResult != null )
        {
            var generatedSources = this.LastSourceGeneratorResult.AdditionalSources
                .ToImmutableDictionary( x => x.Key, x => x.Value.GeneratedSyntaxTree.ToString() );

            return this._serviceHost.PublishGeneratedSourcesAsync(
                projectId,
                generatedSources,
                cancellationToken );
        }
        else
        {
            return Task.CompletedTask;
        }
    }
}