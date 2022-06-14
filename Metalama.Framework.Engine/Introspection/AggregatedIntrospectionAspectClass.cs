// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Introspection;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Metalama.Framework.Engine.Introspection;

internal class AggregatedIntrospectionAspectClass : BaseIntrospectionAspectClass
{
    private readonly IEnumerable<IIntrospectionAspectInstance> _instances;

    public AggregatedIntrospectionAspectClass( IAspectClass aspectClass, IEnumerable<IIntrospectionAspectInstance> instances ) : base( aspectClass )
    {
        this._instances = instances;
    }

    [Memo]
    public override ImmutableArray<IIntrospectionAspectInstance> Instances
        => this._instances.Select(
                x =>
                {
                    var instance = (IntrospectionAspectInstance) x;

                    return new IntrospectionAspectInstance( instance, instance.Compilation, instance.Factory );
                } )
            .ToImmutableArray<IIntrospectionAspectInstance>();
}