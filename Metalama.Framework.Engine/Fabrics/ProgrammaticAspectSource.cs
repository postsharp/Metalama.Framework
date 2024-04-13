// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace Metalama.Framework.Engine.Fabrics;

internal sealed class ProgrammaticAspectSource : IAspectSource
{
    private readonly Func<CompilationModel, OutboundActionCollector, CancellationToken, Task>? _addResultsAction;

    public ProgrammaticAspectSource(
        IAspectClass aspectClass,
        Func<CompilationModel, OutboundActionCollector, CancellationToken, Task> collect )
    {
        this._addResultsAction = collect;
        this.AspectClasses = ImmutableArray.Create( aspectClass );
    }

    public ImmutableArray<IAspectClass> AspectClasses { get; }

    public Task AddAspectInstancesAsync(
        CompilationModel compilation,
        IAspectClass aspectClass,
        OutboundActionCollector collector,
        CancellationToken cancellationToken )
    {
        if ( this._addResultsAction != null )
        {
            return this._addResultsAction.Invoke( compilation, collector, cancellationToken );
        }

        return Task.CompletedTask;
    }
}