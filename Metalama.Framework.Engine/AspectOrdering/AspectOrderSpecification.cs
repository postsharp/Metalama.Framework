// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Metalama.Framework.Engine.AspectOrdering
{
    internal class AspectOrderSpecification
    {
        public AspectOrderSpecification( IEnumerable<string> orderedLayers )
        {
            this.OrderedLayers = orderedLayers.ToImmutableArray();
        }

        public AspectOrderSpecification( AspectOrderAttribute attribute, Location? location )
        {
            var attributeOrderedLayers = attribute.OrderedAspectLayers.ToList();

            // User order of layers is opposite of internal order.
            attributeOrderedLayers.Reverse();

            this.OrderedLayers = attributeOrderedLayers;
            this.DiagnosticLocation = location;
        }

        public Location? DiagnosticLocation { get; }

        public IReadOnlyList<string> OrderedLayers { get; }
    }
}