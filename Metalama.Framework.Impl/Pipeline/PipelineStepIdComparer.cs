// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Impl.AspectOrdering;
using Metalama.Framework.Impl.Aspects;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Metalama.Framework.Impl.Pipeline
{
    /// <summary>
    /// A comparer for <see cref="PipelineStepId"/>. It must be initialized with a collection of <see cref="OrderedAspectLayer"/> because
    /// the <see cref="PipelineStepId"/> type itself does not contain enough information to compare itself.
    /// </summary>
    internal class PipelineStepIdComparer : Comparer<PipelineStepId>
    {
        private readonly ImmutableDictionary<AspectLayerId, OrderedAspectLayer> _orderedAspectLayers;

        public PipelineStepIdComparer( IEnumerable<OrderedAspectLayer> orderedAspectLayers )
        {
            this._orderedAspectLayers = orderedAspectLayers.ToImmutableDictionary( p => p.AspectLayerId, p => p );
        }

        public OrderedAspectLayer GetOrderedAspectLayer( AspectLayerId id ) => this._orderedAspectLayers[id];

        public bool Contains( AspectLayerId id ) => this._orderedAspectLayers.ContainsKey( id );

        public override int Compare( PipelineStepId x, PipelineStepId y )
        {
            if ( x.AspectLayerId != y.AspectLayerId )
            {
                var xOrderedPart = this._orderedAspectLayers[x.AspectLayerId];
                var yOrderedPart = this._orderedAspectLayers[y.AspectLayerId];

                // First order by topological distance.
                if ( xOrderedPart.Order < yOrderedPart.Order )
                {
                    return -1;
                }

                if ( xOrderedPart.Order > yOrderedPart.Order )
                {
                    return 1;
                }

                // If topological distance is identical, order by name.
                var nameOrder = string.Compare( xOrderedPart.AspectName, yOrderedPart.AspectName, StringComparison.Ordinal );

                if ( nameOrder != 0 )
                {
                    return nameOrder;
                }
            }

            // Finally, order by depth in the code mode.
            if ( x.Depth < y.Depth )
            {
                return -1;
            }

            if ( x.Depth > y.Depth )
            {
                return 1;
            }

            if ( !x.Equals( y ) )
            {
                // The steps must be different here, otherwise there would be a duplicate key in the skip list of PipelineStepsState.
                throw new AssertionFailedException();
            }

            return 0;
        }
    }
}