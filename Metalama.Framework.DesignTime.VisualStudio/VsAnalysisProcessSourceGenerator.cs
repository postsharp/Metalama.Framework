// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.DesignTime.VisualStudio.Remoting;
using Metalama.Framework.Engine.Options;
using Metalama.Framework.Engine.Utilities;
using System.Collections.Concurrent;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.VisualStudio;

public class VsAnalysisProcessSourceGenerator : AnalysisProcessSourceGenerator
{
    private static readonly ServiceHost? _serviceHost;

    static VsAnalysisProcessSourceGenerator()
    {
        Logger.Initialize();

        if ( ServiceHost.TryGetPipeName( out var pipeName ) )
        {
            _serviceHost = new ServiceHost( pipeName );
            _serviceHost.Start();
        }
    }

#pragma warning disable CA1001 // ServiceHost is disposable but not owned.
    private class VsAnalysisProcessSourceGeneratorImpl : AnalysisProcessSourceGeneratorImpl
#pragma warning restore CA1001
    {
        public VsAnalysisProcessSourceGeneratorImpl( IProjectOptions projectOptions ) : base( projectOptions )
        {
            if ( _serviceHost != null )
            {
                _serviceHost.ClientConnected += this.OnClientConnected;
            }
        }

        private void OnClientConnected( object sender, ClientConnectedEventArgs e )
        {
            // When a client connects, we update the touch file to force that client to request the info again.
            this.UpdateTouchFile();
        }

        protected override Task PublishGeneratedSourcesAsync( string projectId, CancellationToken cancellationToken )
        {
            if ( _serviceHost != null )
            {
                var generatedSources = this.Sources.ToImmutableDictionary( x => x.Key, x => x.Value.ToString() );

                return _serviceHost.PublishGeneratedSourcesAsync(
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

    protected override SourceGeneratorImpl CreateSourceGeneratorImpl( IProjectOptions projectOptions )
        => new VsAnalysisProcessSourceGeneratorImpl( projectOptions );
}