using Caravela.Framework.Impl.AspectOrdering;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Caravela.Framework.Impl.Pipeline
{
    internal class PipelineStepIdComparer : Comparer<PipelineStepId>
    {
        private ImmutableDictionary<AspectLayerId, OrderedAspectLayer> _orderedAspectLayers;

        public PipelineStepIdComparer( IEnumerable<OrderedAspectLayer> orderedAspectLayers )
        {
            this._orderedAspectLayers = orderedAspectLayers.ToImmutableDictionary( p => p.AspectLayerId, p => p );
        }

        public OrderedAspectLayer GetOrderedAspectLayer( AspectLayerId id ) => this._orderedAspectLayers[id];

        public bool Contains( AspectLayerId id ) => this._orderedAspectLayers.ContainsKey( id );

        public override int Compare( PipelineStepId x, PipelineStepId y )
        {
            var xOrderedPart = this._orderedAspectLayers[x.AspectLayerId];
            var yOrderedPart = this._orderedAspectLayers[y.AspectLayerId];

            if ( xOrderedPart.Order < yOrderedPart.Order )
            {
                return -1;
            }
            else if ( xOrderedPart.Order > yOrderedPart.Order )
            {
                return 1;
            }
            else if ( x.Depth < y.Depth )
            {
                return -1;
            }
            else if ( x.Depth > y.Depth )
            {
                return -1;
            }
            else
            {
                return 0;
            }
        }
    }
}