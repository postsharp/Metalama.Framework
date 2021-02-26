// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Generic;
using System.Collections.Immutable;
using Caravela.Framework.Aspects;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.AspectOrdering
{
    internal class AspectOrderSpecification
    {
        public AspectOrderSpecification( IEnumerable<string> orderedLayers )
        {
            this.OrderedLayers = orderedLayers.ToImmutableArray();
        }

        public AspectOrderSpecification( AspectOrderAttribute attribute, Location? location )
        {
            this.OrderedLayers = attribute.OrderedAspectLayers;
            this.DiagnosticLocation = location;
        }

        public Location? DiagnosticLocation { get; }

        public IReadOnlyList<string> OrderedLayers { get; }
    }
}