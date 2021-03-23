// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Generic;
using Caravela.Framework.Advices;
using Caravela.Framework.Impl.Diagnostics;

namespace Caravela.Framework.Impl
{
    internal record AspectInstanceResult(
        bool Success,
        ImmutableDiagnosticList Diagnostics,
        IReadOnlyList<IAdvice> Advices,
        IReadOnlyList<IAspectSource> AspectSources );
}