// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Advising;

namespace Metalama.Framework.Engine.Transformations;

/// <summary>
/// Represents any transformation.
/// </summary>
internal interface ITransformation : ITransformationBase
{
    Advice ParentAdvice { get; }

    int OrderWithinPipelineStepAndTypeAndAspectInstance { get; set; }

    int OrderWithinPipelineStepAndType { get; set; }

    int OrderWithinPipeline { get; set; }

    TransformationObservability Observability { get; }
}