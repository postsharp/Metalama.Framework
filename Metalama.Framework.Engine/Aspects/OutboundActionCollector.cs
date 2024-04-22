// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.HierarchicalOptions;
using Metalama.Framework.Engine.Validation;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Metalama.Framework.Engine.Aspects;

/// <summary>
/// Collects the outgoing actions of a fabric or aspect (in user code, accessed through <see cref="IAspectBuilder{TAspectTarget}.Outbound"/>
/// i.e. child aspects, aspect exclusions, validators, or options.
/// </summary>
internal sealed class OutboundActionCollector : IDiagnosticAdder
{
    public IDiagnosticAdder Diagnostics { get; }

    private volatile ConcurrentQueue<AspectInstance>? _aspectInstances;
    private volatile ConcurrentQueue<Ref<IDeclaration>>? _exclusions;
    private volatile ConcurrentQueue<AspectRequirement>? _requirements;
    private volatile ConcurrentQueue<ValidatorInstance>? _validators;
    private volatile ConcurrentQueue<HierarchicalOptionsInstance>? _hierarchicalOptions;

    public OutboundActionCollector( IDiagnosticAdder diagnosticAdder )
    {
        this.Diagnostics = diagnosticAdder;
    }

    public IReadOnlyCollection<AspectInstance> AspectInstances => this._aspectInstances ?? (IReadOnlyCollection<AspectInstance>) Array.Empty<AspectInstance>();

    public IReadOnlyCollection<Ref<IDeclaration>> AspectExclusions
        => this._exclusions ?? (IReadOnlyCollection<Ref<IDeclaration>>) Array.Empty<Ref<IDeclaration>>();

    public IReadOnlyCollection<AspectRequirement> AspectRequirements
        => this._requirements ?? (IReadOnlyCollection<AspectRequirement>) Array.Empty<AspectRequirement>();

    public IReadOnlyCollection<ValidatorInstance> Validators => this._validators ?? (IReadOnlyCollection<ValidatorInstance>) Array.Empty<ValidatorInstance>();

    public IReadOnlyCollection<HierarchicalOptionsInstance> HierarchicalOptions
        => this._hierarchicalOptions ?? (IReadOnlyCollection<HierarchicalOptionsInstance>) Array.Empty<HierarchicalOptionsInstance>();

    public void AddAspectInstance( AspectInstance aspectInstance )
    {
        var aspectInstances = this._aspectInstances;

        if ( aspectInstances == null )
        {
            Interlocked.CompareExchange( ref this._aspectInstances, new ConcurrentQueue<AspectInstance>(), null );
            aspectInstances = this._aspectInstances;
        }

        aspectInstances.Enqueue( aspectInstance );
    }

    public void AddExclusion( Ref<IDeclaration> exclusion )
    {
        var exclusions = this._exclusions;

        if ( exclusions == null )
        {
            Interlocked.CompareExchange( ref this._exclusions, new ConcurrentQueue<Ref<IDeclaration>>(), null );
            exclusions = this._exclusions;
        }

        exclusions.Enqueue( exclusion );
    }

    public void AddAspectRequirement( AspectRequirement requirement )
    {
        var requirements = this._requirements;

        if ( requirements == null )
        {
            Interlocked.CompareExchange( ref this._requirements, new ConcurrentQueue<AspectRequirement>(), null );
            requirements = this._requirements;
        }

        requirements.Enqueue( requirement );
    }

    public void AddValidator( ValidatorInstance validator )
    {
        var validators = this._validators;

        if ( validators == null )
        {
            Interlocked.CompareExchange( ref this._validators, new ConcurrentQueue<ValidatorInstance>(), null );
            validators = this._validators;
        }

        validators.Enqueue( validator );
    }

    public void AddOptions( HierarchicalOptionsInstance options )
    {
        var optionsBag = this._hierarchicalOptions;

        if ( optionsBag == null )
        {
            Interlocked.CompareExchange( ref this._hierarchicalOptions, new ConcurrentQueue<HierarchicalOptionsInstance>(), null );
            optionsBag = this._hierarchicalOptions;
        }

        optionsBag.Enqueue( options );
    }

    public void Report( Diagnostic diagnostic ) => this.Diagnostics.Report( diagnostic );
}