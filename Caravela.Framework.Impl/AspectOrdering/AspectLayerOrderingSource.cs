using System.Collections.Generic;
using System.Linq;

namespace Caravela.Framework.Impl.AspectOrdering
{
    internal class AspectLayerOrderingSource : IAspectOrderingSource
    {
        private readonly IReadOnlyList<AspectType> _aspectTypes;

        public AspectLayerOrderingSource( IReadOnlyList<AspectType> aspectTypes )
        {
            this._aspectTypes = aspectTypes;
        }

        public IEnumerable<AspectOrderSpecification> GetAspectOrderSpecification()
            => this._aspectTypes
                .Where( at => at.Layers.Count > 1 )
                .Select( at => new AspectOrderSpecification( at.Layers.Select( l => l.AspectLayerId.FullName ) ) );
    }
}