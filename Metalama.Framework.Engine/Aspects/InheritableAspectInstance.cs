// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace Metalama.Framework.Engine.Aspects;

public partial class InheritableAspectInstance : IAspectInstance
{
    private readonly IAspectClass? _aspectClass;

    public IRef<IDeclaration> TargetDeclaration { get; private set; }

    // This member is not available after deserialization. We would need, if necessary, to have a post-deserialization initialization.
    // The AspectClass is the full type of the aspect, anyway.
    public IAspectClass AspectClass => this._aspectClass ?? throw new InvalidOperationException();

    bool IAspectInstance.IsSkipped => false;

    public ImmutableArray<IAspectInstance> SecondaryInstances { get; private set; }

    ImmutableArray<AspectPredecessor> IAspectInstance.Predecessors => ImmutableArray<AspectPredecessor>.Empty;

    public object? TargetTag { get; private set; }

    public IAspect Aspect { get; private set; }

    public InheritableAspectInstance( IAspectInstance aspectInstance )
    {
        this.TargetDeclaration = aspectInstance.TargetDeclaration;
        this.Aspect = aspectInstance.Aspect;
        this._aspectClass = aspectInstance.AspectClass;
        this.TargetTag = aspectInstance.TargetTag;

        this.SecondaryInstances = aspectInstance.SecondaryInstances
            .Select( i => new InheritableAspectInstance( i ) )
            .ToImmutableArray<IAspectInstance>();
    }

    private InheritableAspectInstance()
    {
        // This is the deserializing constructors. Fields are set by the deserializer, but here
        // we are suppressing warnings.
        this.TargetDeclaration = null!;
        this.SecondaryInstances = default;
        this.Aspect = null!;
    }

    public override string ToString() => $"{nameof(InheritableAspectInstance)}, Aspect={this.Aspect}, Target={this.TargetDeclaration}";
}