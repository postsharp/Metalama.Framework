// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.DesignTime.VisualStudio.Remoting;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.VisualStudio;

public class VsAnalysisProcessSourceGenerator : AnalysisProcessSourceGenerator, IDisposable
{
    private readonly ServiceHost? _serviceHost;

    public VsAnalysisProcessSourceGenerator()
    {
        if ( ServiceHost.TryGetPipeName( out var pipeName ) )
        {
            this._serviceHost = new ServiceHost( pipeName );
            this._serviceHost.Start();
        }
    }

#pragma warning disable CA1001 // ServiceHost is disposable but not owned.
    private class VsAnalysisProcessSourceGeneratorImpl : AnalysisProcessSourceGeneratorImpl
#pragma warning restore CA1001
    {
        private readonly ServiceHost? _serviceHost;

        public VsAnalysisProcessSourceGeneratorImpl( ServiceHost? serviceHost )
        {
            this._serviceHost = serviceHost;
        }

        protected override Task PublishGeneratedSourcesAsync( string projectId, CancellationToken cancellationToken )
        {
            if ( this._serviceHost != null )
            {
                return this._serviceHost.PublishGeneratedSourcesAsync(
                    projectId,
                    this.Sources.ToImmutableDictionary( x => x.Key, x => x.Value.ToString() ),
                    cancellationToken );
            }
            else
            {
                return Task.CompletedTask;
            }
        }
    }

    protected override SourceGeneratorImpl CreateSourceGeneratorImpl() => new VsAnalysisProcessSourceGeneratorImpl( this._serviceHost );

    public void Dispose() => this._serviceHost?.Dispose();
}