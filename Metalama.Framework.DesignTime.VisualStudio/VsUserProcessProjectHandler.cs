// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

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
    private readonly UserProcessServiceHubEndpoint _userProcessEndpoint;
    private readonly ILogger _logger;
    private ImmutableDictionary<string, string>? _sources;

    public VsUserProcessProjectHandler( IServiceProvider serviceProvider, IProjectOptions projectOptions, ProjectKey projectKey ) : base(
        serviceProvider,
        projectOptions,
        projectKey )
    {
        this._logger = serviceProvider.GetLoggerFactory().GetLogger( "DesignTime" );
        this._userProcessEndpoint = serviceProvider.GetRequiredService<UserProcessServiceHubEndpoint>();

        this.Initialize();
    }

#pragma warning disable VSTHRD100 // Avoid "async void" methods.
    private async void Initialize()
    {
        // Since this method is not awaited, we have to handle exceptions here.

        try
        {
            await this._userProcessEndpoint.RegisterProjectCallbackAsync( this.ProjectKey, this );
        }
        catch ( Exception e )
        {
            DesignTimeExceptionHandler.ReportException( e );
        }
    }
#pragma warning restore VSTHRD100 // Avoid "async void" methods.

    public override SourceGeneratorResult GenerateSources( Compilation compilation, CancellationToken cancellationToken )
    {
        if ( this._sources == null )
        {
            // If we have not received the source yet, see if it was received by the client before we were created.
            if ( this._userProcessEndpoint.TryGetGenerateSourcesIfAvailable( this.ProjectKey, out var sources ) )
            {
                this._logger.Trace?.Log( $"Generated sources for '{this.ProjectKey}' were retrieved from ServiceClient." );
                this._sources = sources;
            }
            else
            {
                this._logger.Warning?.Log( $"Information about generated sources for '{this.ProjectKey}' is not available." );

                return SourceGeneratorResult.Empty;
            }
        }

        return new TextSourceGeneratorResult( this._sources.AssertNotNull() );
    }

    Task IProjectHandlerCallback.PublishGeneratedCodeAsync(
        ProjectKey projectKey,
        ImmutableDictionary<string, string> sources,
        CancellationToken cancellationToken )
    {
        this._sources = sources;

        return Task.CompletedTask;
    }
}