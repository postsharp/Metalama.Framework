// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Collections;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Utilities.Roslyn;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

namespace Metalama.Framework.Engine.Pipeline;

/// <summary>
/// An <see cref="IAspectSource"/> that stores aspect sources that are not a part of the current <see cref="PipelineStage"/>.
/// </summary>
internal sealed class OverflowAspectSource : IAspectSource
{
    private readonly ConcurrentLinkedList<(IAspectSource Source, IAspectClass AspectClass)> _aspectSources = new();

    public ImmutableArray<IAspectClass> AspectClasses => this._aspectSources.SelectAsEnumerable( a => a.AspectClass ).Distinct().ToImmutableArray();

    public AspectSourceResult GetAspectInstances(
        CompilationModel compilation,
        IAspectClass aspectClass,
        IDiagnosticAdder diagnosticAdder,
        CancellationToken cancellationToken )
    {
        var aspectTypeSymbol = compilation.RoslynCompilation.GetTypeByMetadataName( aspectClass.FullName ).AssertNotNull();

        var aspectSourceResults =
            this._aspectSources
                .Where( s => s.AspectClass.FullName.Equals( aspectTypeSymbol.GetReflectionFullName().AssertNotNull(), StringComparison.Ordinal ) )
                .Select( a => a.Source )
                .Distinct()
                .Select( a => a.GetAspectInstances( compilation, aspectClass, diagnosticAdder, cancellationToken ) )
                .ToList();

        return new AspectSourceResult( aspectSourceResults.SelectMany( x => x.AspectInstances ), aspectSourceResults.SelectMany( x => x.Exclusions ) );
    }

    public void Add( IAspectSource aspectSource, IAspectClass aspectClass ) => this._aspectSources.Add( (aspectSource, aspectClass) );
}