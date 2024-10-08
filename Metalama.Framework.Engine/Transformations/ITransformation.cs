// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.Aspects;
using System;

namespace Metalama.Framework.Engine.Transformations;

/// <summary>
/// Represents any transformation.
/// </summary>
internal interface ITransformation : ITransformationBase
{
    [Obsolete( "We want to get rid of this" )]
    Advice ParentAdvice { get; }

    IAspectInstanceInternal AspectInstance { get; }

    int OrderWithinPipelineStepAndTypeAndAspectInstance { get; set; }

    int OrderWithinPipelineStepAndType { get; set; }

    int OrderWithinPipeline { get; set; }

    TransformationObservability Observability { get; }
}