using System.Collections.Generic;
using System.Collections.Immutable;
using Caravela.Framework.Impl.AspectOrdering;

namespace Caravela.Framework.Impl.Pipeline
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