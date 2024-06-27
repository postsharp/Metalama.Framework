// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Introspection;
using System;

namespace Metalama.Framework.Engine.AdviceImpl.Attributes;

internal sealed class AddAnnotationTransformation : BaseTransformation
{
    public AddAnnotationTransformation( AddAnnotationAdvice advice, IDeclaration declaration, AnnotationInstance annotationInstance ) : base( advice )
    {
        this.TargetDeclaration = declaration;
        this.AnnotationInstance = annotationInstance;
    }

    public AnnotationInstance AnnotationInstance { get; }

    public override IDeclaration TargetDeclaration { get; }

    public override IntrospectionTransformationKind TransformationKind => IntrospectionTransformationKind.AddAnnotation;

    public override FormattableString ToDisplayString() => $"Adding annotation '{this.AnnotationInstance}' to '{this.TargetDeclaration}'.";

    public override TransformationObservability Observability => TransformationObservability.CompileTimeOnly;
}