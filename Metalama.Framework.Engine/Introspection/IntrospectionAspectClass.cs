// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Introspection;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace Metalama.Framework.Engine.Introspection;

internal class IntrospectionAspectClass : BaseIntrospectionAspectClass
{
    private readonly ImmutableArray<AspectInstanceResult> _aspectInstanceResults;
    private readonly Func<AspectInstanceResult, IntrospectionAspectInstance> _evaluatedAspectInstanceFactory;

    public IntrospectionAspectClass(
        IAspectClass aspectClass,
        ImmutableArray<AspectInstanceResult> aspectInstanceResults,
        Func<AspectInstanceResult, IntrospectionAspectInstance> evaluatedAspectInstanceFactory )
        : base( aspectClass )
    {
        this._aspectInstanceResults = aspectInstanceResults;
        this._evaluatedAspectInstanceFactory = evaluatedAspectInstanceFactory;
    }

    [Memo]
    public override ImmutableArray<IIntrospectionAspectInstance> Instances
        => this._aspectInstanceResults
            .Select( x => this._evaluatedAspectInstanceFactory( x ) )
            .ToImmutableArray<IIntrospectionAspectInstance>();
}