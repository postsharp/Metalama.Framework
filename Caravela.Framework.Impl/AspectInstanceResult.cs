// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Advices;
using Caravela.Framework.Impl.Diagnostics;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Caravela.Framework.Impl
{
    internal class AspectInstanceResult
    {
        public bool Success { get; }

        public ImmutableUserDiagnosticList Diagnostics { get; }

        public IReadOnlyList<IAdvice> Advices { get; }

        public IReadOnlyList<IAspectSource> AspectSources { get; }

        public ImmutableDictionary<string, object?> Tags { get; }

        public AspectInstanceResult(
            bool success,
            ImmutableUserDiagnosticList diagnostics,
            IReadOnlyList<IAdvice> advices,
            IReadOnlyList<IAspectSource> aspectSources,
            ImmutableDictionary<string, object?> tags )
        {
            this.Success = success;
            this.Diagnostics = diagnostics;
            this.Advices = advices;
            this.AspectSources = aspectSources;
            this.Tags = tags;
        }
    }
}