// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.AspectOrdering;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.Aspects;

internal class AspectLayerIdComparer : IComparer<AspectLayerId>
{
    private readonly Dictionary<AspectLayerId, OrderedAspectLayer> _aspectLayers;

    public AspectLayerIdComparer( IEnumerable<OrderedAspectLayer> aspectLayers )
    {
        this._aspectLayers = aspectLayers.ToDictionary( x => x.AspectLayerId, x => x );
    }

    public int Compare( AspectLayerId x, AspectLayerId y ) => this._aspectLayers[x].Order.CompareTo( this._aspectLayers[y].Order );
}