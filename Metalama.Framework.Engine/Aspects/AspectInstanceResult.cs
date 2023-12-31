﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.HierarchicalOptions;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Validation;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.Aspects
{
    internal sealed class AspectInstanceResult
    {
        public IAspectInstance AspectInstance { get; }

        public AdviceOutcome Outcome { get; }

        public ImmutableUserDiagnosticList Diagnostics { get; }

        public ImmutableArray<ITransformation> Transformations { get; }

        public ImmutableArray<IAspectSource> AspectSources { get; }

        public ImmutableArray<IValidatorSource> ValidatorSources { get; }

        public ImmutableArray<IHierarchicalOptionsSource> OptionsSources { get; }

        public AspectInstanceResult(
            IAspectInstance aspectInstance,
            AdviceOutcome outcome,
            ImmutableUserDiagnosticList diagnostics,
            ImmutableArray<ITransformation> transformations,
            ImmutableArray<IAspectSource> aspectSources,
            ImmutableArray<IValidatorSource> validatorSources,
            ImmutableArray<IHierarchicalOptionsSource> optionsSources )
        {
            this.AspectInstance = aspectInstance;
            this.Outcome = outcome;
            this.Diagnostics = diagnostics;
            this.Transformations = transformations;
            this.AspectSources = aspectSources;
            this.ValidatorSources = validatorSources;
            this.OptionsSources = optionsSources;
        }

        public AspectInstanceResult WithAdditionalDiagnostics( ImmutableUserDiagnosticList diagnostics )
            => new(
                this.AspectInstance,
                this.Outcome,
                this.Diagnostics.Concat( diagnostics ),
                this.Transformations,
                this.AspectSources,
                this.ValidatorSources,
                this.OptionsSources );
    }
}