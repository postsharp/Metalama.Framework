using Metalama.Framework.DesignTime;
using Metalama.Framework.DesignTime.Rpc;
using System.Collections.Concurrent;
using System.Collections.Immutable;

namespace Metalama.Framework.Tests.UnitTests.DesignTime;

internal class ProjectHandlerObserver : IProjectHandlerObserver
{
    public BlockingCollection<(ProjectKey ProjectKey, ImmutableDictionary<string, string> Sources)> PublishedSources { get; } = new();

    public BlockingCollection<(ProjectKey ProjectKey, string Content)> PublishedTouchFiles { get; } = new();

    public void OnGeneratedCodePublished( ProjectKey projectKey, ImmutableDictionary<string, string> sources )
        => this.PublishedSources.Add( (projectKey, sources) );

    public void OnTouchFileWritten( ProjectKey projectKey, string content ) => this.PublishedTouchFiles.Add( (projectKey, content) );

    public void Reset()
    {
        while ( this.PublishedTouchFiles.TryTake( out _ ) ) { }

        while ( this.PublishedSources.TryTake( out _ ) ) { }
    }
}