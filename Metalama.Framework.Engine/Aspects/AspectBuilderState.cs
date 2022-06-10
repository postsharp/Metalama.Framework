// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Advices;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Engine.Validation;
using System;
using System.Collections.Immutable;
using System.Threading;

namespace Metalama.Framework.Engine.Aspects;

internal class AspectBuilderState
{
    public IServiceProvider ServiceProvider { get; }

    public UserDiagnosticSink Diagnostics { get; }

    public AspectPipelineConfiguration Configuration { get; }

    public ImmutableArray<IAspectSource> AspectSources { get; set; } = ImmutableArray<IAspectSource>.Empty;

    public ImmutableArray<IValidatorSource> ValidatorSources { get; set; } = ImmutableArray<IValidatorSource>.Empty;

    public CancellationToken CancellationToken { get; }

    public IAspectInstance AspectInstance { get; }

    public AdviceFactoryState AdviceFactoryState { get; }

    public bool IsAspectSkipped { get; set; }

    public AspectBuilderState(
        IServiceProvider serviceProvider,
        UserDiagnosticSink diagnostics,
        AspectPipelineConfiguration configuration,
        IAspectInstance aspectInstance,
        AdviceFactoryState adviceFactoryState,
        CancellationToken cancellationToken )
    {
        this.ServiceProvider = serviceProvider;
        this.Diagnostics = diagnostics;
        this.Configuration = configuration;
        this.CancellationToken = cancellationToken;
        this.AspectInstance = aspectInstance;
        this.CancellationToken = cancellationToken;
        this.AdviceFactoryState = adviceFactoryState;
    }

    internal AspectInstanceResult ToResult()
    {
        var success = this.Diagnostics.ErrorCount == 0;

        return success && !this.IsAspectSkipped
            ? new AspectInstanceResult(
                this.AspectInstance,
                success,
                this.Diagnostics.ToImmutable(),
                this.AdviceFactoryState.Advices.ToImmutableArray(),
                this.AspectSources,
                this.ValidatorSources )
            : new AspectInstanceResult(
                this.AspectInstance,
                success,
                this.Diagnostics.ToImmutable(),
                ImmutableArray<Advice>.Empty,
                ImmutableArray<IAspectSource>.Empty,
                ImmutableArray<IValidatorSource>.Empty );
    }
}