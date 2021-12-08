// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Impl.Diagnostics;
using System.Collections.Generic;

namespace Metalama.Framework.Impl.AspectOrdering
{
    internal interface IAspectOrderingSource
    {
        IEnumerable<AspectOrderSpecification> GetAspectOrderSpecification( IDiagnosticAdder diagnosticAdder );
    }
}