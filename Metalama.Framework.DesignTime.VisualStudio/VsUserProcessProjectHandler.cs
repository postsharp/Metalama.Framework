// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Framework.DesignTime.SourceGeneration;
using Metalama.Framework.DesignTime.VisualStudio.Remoting;
using Metalama.Framework.Engine;
using Metalama.Framework.Engine.Options;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.VisualStudio;

/// <summary>
/// Implementation of <see cref="ProjectHandler"/> in the Visual Studio user process. It receives generated source code
/// from the analysis process.
/// </summary>
internal class VsUserProcessProjectHandler : ProjectHandler, IProjectHandlerCallback
{
    private readonly UserProcessEndpoint _userProcessEndpoint;
    private readonly ILogger _logger;
    private ImmutableDictionary<string, string>? _sources;

    public VsUserProcessProjectHandler( IServiceProvider serviceProvider, IProjectOptions projectOptions ) : base( serviceProvider, projectOptions )
    {
        this._logger = serviceProvider.GetLoggerFactory().GetLogger( "DesignTime" );
        this._userProcessEndpoint = serviceProvider.GetRequiredService<UserProcessEndpoint>();

        _ = this._userProcessEndpoint.RegisterProjectHandlerAsync( projectOptions.ProjectId, this );
    }

    public override SourceGeneratorResult GenerateSources( Compilation compilation, CancellationToken cancellationToken )
    {
        if ( this._sources == null )
        {
            // If we have not received the source yet, see if it was received by the client before we were created.
            if ( this._userProcessEndpoint.TryGetUnhandledSources( this.ProjectOptions.ProjectId, out var sources ) )
            {
                this._logger.Trace?.Log( $"Generated sources for '{this.ProjectOptions.ProjectId}' were retrieved from ServiceClient." );
                this._sources = sources;
            }
            else
            {
                this._logger.Warning?.Log( $"Information about generated sources for '{this.ProjectOptions.ProjectId}' is not available." );

                return SourceGeneratorResult.Empty;
            }
        }

        return new TextSourceGeneratorResult( this._sources.AssertNotNull() );
    }

    Task IProjectHandlerCallback.PublishGeneratedCodeAsync( string projectId, ImmutableDictionary<string, string> sources, CancellationToken cancellationToken )
    {
        this._sources = sources;

        return Task.CompletedTask;
    }
}