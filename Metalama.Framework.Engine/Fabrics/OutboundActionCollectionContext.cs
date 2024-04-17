// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using System.Threading;

namespace Metalama.Framework.Engine.Fabrics;

internal class OutboundActionCollectionContext : DeclarationSelectionContext
{
    public OutboundActionCollector Collector { get; }

    public OutboundActionCollectionContext( OutboundActionCollector collector, CompilationModel compilation, CancellationToken cancellationToken ) : base(
        compilation,
        cancellationToken )
    {
        this.Collector = collector;
    }
}