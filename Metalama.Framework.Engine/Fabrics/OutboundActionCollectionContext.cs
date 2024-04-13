using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using System.Threading;

namespace Metalama.Framework.Engine.Fabrics;

internal class OutboundActionCollectionContext
{
    public CancellationToken CancellationToken { get; }

    public OutboundActionCollector Collector { get; }

    public CompilationModel Compilation { get; }

    public OutboundActionCollectionContext( OutboundActionCollector collector, CompilationModel compilation, CancellationToken cancellationToken )
    {
        this.CancellationToken = cancellationToken;
        this.Collector = collector;
        this.Compilation = compilation;
    }
}