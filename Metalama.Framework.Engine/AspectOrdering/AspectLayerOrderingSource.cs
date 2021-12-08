// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.Diagnostics;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.AspectOrdering
{
    internal class AspectLayerOrderingSource : IAspectOrderingSource
    {
        private readonly IReadOnlyList<AspectClass> _aspectTypes;

        public AspectLayerOrderingSource( IReadOnlyList<AspectClass> aspectTypes )
        {
            this._aspectTypes = aspectTypes;
        }

        public IEnumerable<AspectOrderSpecification> GetAspectOrderSpecification( IDiagnosticAdder diagnosticAdder )
            => this._aspectTypes
                .Where( at => at.Layers.Count > 1 )
                .Select( at => new AspectOrderSpecification( at.Layers.Select( l => l.AspectLayerId.FullName ) ) );
    }
}