// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

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

internal sealed class AspectResultCollector : IDiagnosticAdder
{
    public IDiagnosticAdder Diagnostics { get; }

    private volatile ConcurrentBag<AspectInstance>? _aspectInstances;
    private volatile ConcurrentBag<Ref<IDeclaration>>? _exclusions;
    private volatile ConcurrentBag<AspectRequirement>? _requirements;
    private volatile ConcurrentBag<ValidatorInstance>? _validators;
    private volatile ConcurrentBag<HierarchicalOptionsInstance>? _hierarchicalOptions;

    public AspectResultCollector( IDiagnosticAdder diagnosticAdder )
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
            Interlocked.CompareExchange( ref this._aspectInstances, new ConcurrentBag<AspectInstance>(), null );
            aspectInstances = this._aspectInstances;
        }

        aspectInstances.Add( aspectInstance );
    }

    public void AddExclusion( Ref<IDeclaration> exclusion )
    {
        var exclusions = this._exclusions;

        if ( exclusions == null )
        {
            Interlocked.CompareExchange( ref this._exclusions, new ConcurrentBag<Ref<IDeclaration>>(), null );
            exclusions = this._exclusions;
        }

        exclusions.Add( exclusion );
    }

    public void AddAspectRequirement( AspectRequirement requirement )
    {
        var requirements = this._requirements;

        if ( requirements == null )
        {
            Interlocked.CompareExchange( ref this._requirements, new ConcurrentBag<AspectRequirement>(), null );
            requirements = this._requirements;
        }

        requirements.Add( requirement );
    }

    public void AddValidator( ValidatorInstance validator )
    {
        var validators = this._validators;

        if ( validators == null )
        {
            Interlocked.CompareExchange( ref this._validators, new ConcurrentBag<ValidatorInstance>(), null );
            validators = this._validators;
        }

        validators.Add( validator );
    }

    public void AddOptions( HierarchicalOptionsInstance options )
    {
        var optionsBag = this._hierarchicalOptions;

        if ( optionsBag == null )
        {
            Interlocked.CompareExchange( ref this._hierarchicalOptions, new ConcurrentBag<HierarchicalOptionsInstance>(), null );
            optionsBag = this._hierarchicalOptions;
        }

        optionsBag.Add( options );
    }

    public void Report( Diagnostic diagnostic ) => this.Diagnostics.Report( diagnostic );
}