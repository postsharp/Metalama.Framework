// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Advices;
using Caravela.Framework.Impl.Diagnostics;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.Aspects
{
    internal class AspectInstanceResult
    {
        public bool Success { get; }

        public ImmutableUserDiagnosticList Diagnostics { get; }

        public IReadOnlyList<Advice> Advices { get; }

        public IReadOnlyList<IAspectSource> AspectSources { get; }

        public AspectInstanceResult(
            bool success,
            ImmutableUserDiagnosticList diagnostics,
            IReadOnlyList<Advice> advices,
            IReadOnlyList<IAspectSource> aspectSources )
        {
            this.Success = success;
            this.Diagnostics = diagnostics;
            this.Advices = advices;
            this.AspectSources = aspectSources;
        }
    }
}