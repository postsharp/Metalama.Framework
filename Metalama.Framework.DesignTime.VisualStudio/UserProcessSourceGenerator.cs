// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.DesignTime.VisualStudio.Remoting;
using Metalama.Framework.Engine.Options;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.VisualStudio;

public class UserProcessSourceGenerator : DesignTimeSourceGenerator, IDisposable
{
    private readonly ServiceClient _serviceClient;

    public UserProcessSourceGenerator()
    {
        this._serviceClient = new ServiceClient();
        this._serviceClient.GeneratedCodePublished += this.OnGeneratedCodePublished;
        _ = this._serviceClient.ConnectAsync();
    }

    private void OnGeneratedCodePublished( object sender, GeneratedCodeChangedEventArgs e )
    {
        Logger.DesignTime.Trace?.Log( $"Received new generated code from the remote host for project '{e.ProjectId}'." );

        if ( this.TryGetImpl( e.ProjectId, out var generator ) )
        {
            ((InteractiveProcessSourceGeneratorImpl) generator).Sources = e.GeneratedSources;
        }
        else
        {
            Logger.DesignTime.Warning?.Log( $"Cannot find the implementation for project '{e.ProjectId}'." );
        }
    }

    private class InteractiveProcessSourceGeneratorImpl : SourceGeneratorImpl
    {
        public ImmutableDictionary<string, string> Sources { get; set; } = ImmutableDictionary<string, string>.Empty;

        public override void GenerateSources( IProjectOptions projectOptions, Compilation compilation, GeneratorExecutionContext context )
        {
            foreach ( var source in this.Sources )
            {
                context.AddSource( source.Key, source.Value );
            }
        }
    }

    protected override SourceGeneratorImpl CreateSourceGeneratorImpl() => new InteractiveProcessSourceGeneratorImpl();

    public void Dispose() => this._serviceClient?.Dispose();
}