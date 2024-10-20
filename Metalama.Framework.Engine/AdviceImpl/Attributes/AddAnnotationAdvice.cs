﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Advising;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Transformations;
using System;

namespace Metalama.Framework.Engine.AdviceImpl.Attributes;

internal sealed class AddAnnotationAdvice : Advice<AddAnnotationAdviceResult>
{
    private readonly AnnotationInstance _annotationInstance;

    public AddAnnotationAdvice( AdviceConstructorParameters parameters, AnnotationInstance annotationInstance )
        : base( parameters )
    {
        this._annotationInstance = annotationInstance;
    }

    public override AdviceKind AdviceKind => AdviceKind.AddAnnotation;

    protected override AddAnnotationAdviceResult Implement(
        ProjectServiceProvider serviceProvider,
        CompilationModel compilation,
        Action<ITransformation> addTransformation )
    {
        addTransformation( new AddAnnotationTransformation( this, this.TargetDeclaration.GetTarget( compilation ), this._annotationInstance ) );

        return new AddAnnotationAdviceResult();
    }
}