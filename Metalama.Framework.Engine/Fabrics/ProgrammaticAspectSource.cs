// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Aspects;
using System;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace Metalama.Framework.Engine.Fabrics;

internal sealed class ProgrammaticAspectSource : IAspectSource
{
    private readonly Func<OutboundActionCollectionContext, Task> _addResultsAction;

    public ProgrammaticAspectSource(
        IAspectClass aspectClass,
        Func<OutboundActionCollectionContext, Task> collect )
    {
        this._addResultsAction = collect;
        this.AspectClasses = ImmutableArray.Create( aspectClass );
    }

    public ImmutableArray<IAspectClass> AspectClasses { get; }

    public Task CollectAspectInstancesAsync(
        IAspectClass aspectClass,
        OutboundActionCollectionContext context )
    {
        if ( this._addResultsAction != null )
        {
            return this._addResultsAction.Invoke( context );
        }

        return Task.CompletedTask;
    }
}