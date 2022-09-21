// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.AspectOrdering;
using Metalama.Framework.Engine.Transformations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.Linking;

internal class TransformationLinkerOrderComparer : Comparer<ITransformation>
{
    public static TransformationLinkerOrderComparer Instance { get; } = new();

    private TransformationLinkerOrderComparer() { }

    public override int Compare( ITransformation? x, ITransformation? y )
    {
        if ( x == y )
        {
            return 0;
        }

        if ( x == null )
        {
            return 1;
        }
        else if ( y == null )
        {
            return -1;
        }

        // Sort by pipeline order.
        var aspectLayerComparison = x.OrderWithinPipeline.CompareTo( y.OrderWithinPipeline );

        if ( aspectLayerComparison != 0 )
        {
            return aspectLayerComparison;
        }

        // Sort by target type (the aspect framework process all types of the same pipeline step in parallel, but at this point we need strong ordering). 
        var targetTypeComparison = StringComparer.Ordinal.Compare(
            x.TargetDeclaration.GetClosestNamedType()?.FullName,
            y.TargetDeclaration.GetClosestNamedType()?.FullName );

        if ( targetTypeComparison != 0 )
        {
            return targetTypeComparison;
        }

        // Sort by processing order within the type (as set with the pipeline).
        var orderWithinTypeComparison = x.OrderWithinPipelineStepAndType.CompareTo( y.OrderWithinPipelineStepAndType );

        if ( orderWithinTypeComparison != 0 )
        {
            return orderWithinTypeComparison;
        }

        // Sort by order within the aspect instance (i.e. the order in which the advice were added).
        var aspectInstanceComparison =
            x.OrderWithinPipelineStepAndTypAndAspectInstance.CompareTo( y.OrderWithinPipelineStepAndTypAndAspectInstance );

        if ( aspectInstanceComparison != 0 )
        {
            return aspectInstanceComparison;
        }

        // At this point, there should be no ambiguity.
        throw new AssertionFailedException();
    }
}