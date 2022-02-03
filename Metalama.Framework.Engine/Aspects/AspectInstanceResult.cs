// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Advices;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Validation;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.Aspects
{
    internal class AspectInstanceResult
    {
        public IAspectInstance AspectInstance { get; }

        public bool Success { get; }

        public ImmutableUserDiagnosticList Diagnostics { get; }

        public ImmutableArray<Advice> ProgrammaticAdvices { get; }

        public IReadOnlyList<IAspectSource> AspectSources { get; }

        public ImmutableArray<IValidatorSource> ValidatorSources { get; }

        public AspectInstanceResult(
            IAspectInstance aspectInstance,
            bool success,
            ImmutableUserDiagnosticList diagnostics,
            ImmutableArray<Advice> declarativeAdvices,
            ImmutableArray<IAspectSource> aspectSources,
            ImmutableArray<IValidatorSource> validatorSources )
        {
            this.AspectInstance = aspectInstance;
            this.Success = success;
            this.Diagnostics = diagnostics;
            this.ProgrammaticAdvices = declarativeAdvices;
            this.AspectSources = aspectSources;
            this.ValidatorSources = validatorSources;
        }
    }
}