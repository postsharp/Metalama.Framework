// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Advices;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Validation;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.Aspects
{
    internal class AspectInstanceResult
    {
        public IAspectInstance AspectInstance { get; }

        public bool Success { get; }

        public ImmutableUserDiagnosticList Diagnostics { get; }

        public ImmutableArray<Advice> Advices { get; }

        public ImmutableArray<IAspectSource> AspectSources { get; }

        public ImmutableArray<IValidatorSource> ValidatorSources { get; }

        public AspectInstanceResult(
            IAspectInstance aspectInstance,
            bool success,
            ImmutableUserDiagnosticList diagnostics,
            ImmutableArray<Advice> advices,
            ImmutableArray<IAspectSource> aspectSources,
            ImmutableArray<IValidatorSource> validatorSources )
        {
            this.AspectInstance = aspectInstance;
            this.Success = success;
            this.Diagnostics = diagnostics;
            this.Advices = advices;
            this.AspectSources = aspectSources;
            this.ValidatorSources = validatorSources;
        }

        public AspectInstanceResult WithAdditionalDiagnostics( ImmutableUserDiagnosticList diagnostics )
            => new( this.AspectInstance, this.Success, this.Diagnostics.Concat( diagnostics ), this.Advices, this.AspectSources, this.ValidatorSources );
    }
}