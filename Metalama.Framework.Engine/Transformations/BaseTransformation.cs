// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Introspection;
using System;

namespace Metalama.Framework.Engine.Transformations;

internal abstract class BaseTransformation : ITransformation
{
    protected BaseTransformation( Advice advice )
    {
        this.ParentAdvice = advice;
    }

    /// <summary>
    /// Gets the declaration that is transformed, or the declaration into which a new declaration is being introduced. 
    /// </summary>
    public abstract IDeclaration TargetDeclaration { get; }

    IAspectClass ITransformationBase.AspectClass => this.ParentAdvice.AspectInstance.AspectClass;

    public Advice ParentAdvice { get; }

    public int OrderWithinPipelineStepAndTypeAndAspectInstance { get; set; }

    public int OrderWithinPipelineStepAndType { get; set; }

    public int OrderWithinPipeline { get; set; }

    public abstract TransformationObservability Observability { get; }

    public abstract IntrospectionTransformationKind TransformationKind { get; }

    public abstract FormattableString ToDisplayString();
}