// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Fabrics;
using System;
using System.Threading.Tasks;

namespace Metalama.Framework.Engine.HierarchicalOptions;

internal sealed class ProgrammaticHierarchicalOptionsSource : IHierarchicalOptionsSource
{
    private readonly Func<OutboundActionCollectionContext, Task> _collectOptionsAction;

    public ProgrammaticHierarchicalOptionsSource( Func<OutboundActionCollectionContext, Task> collectOptionsAction )
    {
        this._collectOptionsAction = collectOptionsAction;
    }

    public Task CollectOptionsAsync( OutboundActionCollectionContext context )
    {
        return this._collectOptionsAction( context );
    }
}