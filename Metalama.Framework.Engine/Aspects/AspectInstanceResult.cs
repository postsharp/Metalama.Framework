// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Advices;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Validation;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.Aspects
{
    internal class AspectInstanceResult
    {
        public bool Success { get; }

        public ImmutableUserDiagnosticList Diagnostics { get; }

        public IReadOnlyList<Advice> Advices { get; }

        public IReadOnlyList<IAspectSource> AspectSources { get; }

        public ImmutableArray<ValidatorSource> ValidatorSources { get; }

        public AspectInstanceResult(
            bool success,
            ImmutableUserDiagnosticList diagnostics,
            ImmutableArray<Advice> advices,
            ImmutableArray<IAspectSource> aspectSources,
            ImmutableArray<ValidatorSource> validatorSources )
        {
            this.Success = success;
            this.Diagnostics = diagnostics;
            this.Advices = advices;
            this.AspectSources = aspectSources;
            this.ValidatorSources = validatorSources;
        }
    }
}