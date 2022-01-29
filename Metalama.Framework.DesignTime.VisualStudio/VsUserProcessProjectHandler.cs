// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Framework.DesignTime.VisualStudio.Remoting;
using Metalama.Framework.Engine.Options;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.VisualStudio;

internal class VsUserProcessProjectHandler : ProjectHandler, IProjectHandlerCallback
{
    private readonly ServiceClient _serviceClient;
    private readonly ILogger _logger;

    public VsUserProcessProjectHandler( IServiceProvider serviceProvider, IProjectOptions projectOptions ) : base( serviceProvider, projectOptions )
    {
        this._logger = serviceProvider.GetLoggerFactory().GetLogger( "DesignTime" );
        this._serviceClient = serviceProvider.GetRequiredService<ServiceClient>();

        _ = this._serviceClient.RegisterProjectHandlerAsync( projectOptions.ProjectId, this );
    }

    public ImmutableDictionary<string, string>? Sources { get; set; }

    public override void GenerateSources( Compilation compilation, GeneratorExecutionContext context )
    {
        if ( this.Sources == null )
        {
            // If we have not received the source yet, see if it was received by the client before we were created.
            if ( this._serviceClient.TryGetUnhandledSources( this.ProjectOptions.ProjectId, out var sources ) )
            {
                this._logger.Trace?.Log( $"Generated sources for '{this.ProjectOptions.ProjectId}' were retrieved from ServiceClient." );
                this.Sources = sources;
            }
            else
            {
                this._logger.Warning?.Log( $"Information about generated sources for '{this.ProjectOptions.ProjectId}' is not available." );

                return;
            }
        }

        foreach ( var source in this.Sources! )
        {
            context.AddSource( source.Key, source.Value );
        }
    }

    Task IProjectHandlerCallback.PublishGeneratedCodeAsync( string projectId, ImmutableDictionary<string, string> sources, CancellationToken cancellationToken )
    {
        this.Sources = sources;

        return Task.CompletedTask;
    }
}