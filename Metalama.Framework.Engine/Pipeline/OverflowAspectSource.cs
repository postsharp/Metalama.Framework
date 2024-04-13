// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Collections;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Metalama.Framework.Engine.Pipeline;

/// <summary>
/// An <see cref="IAspectSource"/> that stores aspect sources that are not a part of the current <see cref="PipelineStage"/>.
/// </summary>
internal sealed class OverflowAspectSource : IAspectSource
{
    private readonly ConcurrentLinkedList<(IAspectSource Source, IAspectClass AspectClass)> _aspectSources = new();

    public ImmutableArray<IAspectClass> AspectClasses => this._aspectSources.SelectAsReadOnlyCollection( a => a.AspectClass ).Distinct().ToImmutableArray();

    public Task AddAspectInstancesAsync(
        CompilationModel compilation,
        IAspectClass aspectClass,
        AspectResultCollector collector,
        CancellationToken cancellationToken )
    {
        var tasks =
            this._aspectSources
                .Where( s => s.AspectClass.FullName == aspectClass.FullName )
                .Select( a => a.Source )
                .Distinct()
                .Select( a => a.AddAspectInstancesAsync( compilation, aspectClass, collector, cancellationToken ) );

        return Task.WhenAll( tasks );
    }

    public void Add( IAspectSource aspectSource, IAspectClass aspectClass ) => this._aspectSources.Add( (aspectSource, aspectClass) );
}