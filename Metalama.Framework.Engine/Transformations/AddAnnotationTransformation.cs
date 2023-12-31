﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Introspection;
using System;

namespace Metalama.Framework.Engine.Transformations;

internal sealed class AddAnnotationTransformation : BaseTransformation
{
    public AddAnnotationTransformation( AddAnnotationAdvice advice, IDeclaration declaration, AnnotationInstance annotationInstance ) : base( advice )
    {
        this.TargetDeclaration = declaration;
        this.AnnotationInstance = annotationInstance;
    }

    public AnnotationInstance AnnotationInstance { get; }

    public override IDeclaration TargetDeclaration { get; }

    public override TransformationKind TransformationKind => TransformationKind.AddAnnotation;

    public override FormattableString ToDisplayString() => $"Adding annotation '{this.AnnotationInstance}' to '{this.TargetDeclaration}'.";

    public override TransformationObservability Observability => TransformationObservability.CompileTimeOnly;
}