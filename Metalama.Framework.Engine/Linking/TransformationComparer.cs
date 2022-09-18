// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.Transformations;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.Linking;

internal class TransformationComparer : Comparer<ITransformation>
{
    private readonly AspectLayerIdComparer _aspectLayerIdComparer;

    public TransformationComparer( AspectLayerIdComparer aspectLayerIdComparer )
    {
        this._aspectLayerIdComparer = aspectLayerIdComparer;
    }

    public override int Compare( ITransformation x, ITransformation y )
    {
        if ( x == y )
        {
            return 0;
        }

        var aspectLayerComparison = this._aspectLayerIdComparer.Compare( x.ParentAdvice.AspectLayerId, y.ParentAdvice.AspectLayerId );

        if ( aspectLayerComparison != 0 )
        {
            return aspectLayerComparison;
        }

        var aspectInstanceComparison =
            x.ParentAdvice.Aspect.OrderWithinTypeAndAspectLayer.CompareTo( y.ParentAdvice.Aspect.OrderWithinTypeAndAspectLayer );

        if ( aspectInstanceComparison != 0 )
        {
            return aspectInstanceComparison;
        }

        var withinAspectInstanceComparison = x.OrderWithinAspectInstance.CompareTo( y.OrderWithinAspectInstance );

        if ( withinAspectInstanceComparison == 0 )
        {
            // Order undetermined. 
            // This should not happen in production, but it can happen in the linker because the OrderWithinTypeAndAspectLayer and OrderWithinAspectInstance
            // properties are not properly mocked.
            // TODO: throw AssertionFailedException.
            return 0;
        }

        return withinAspectInstanceComparison;
    }
}