// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Advising;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Transformations;
using System;

namespace Metalama.Framework.Engine.Advising;

internal sealed class AddAnnotationAdvice : Advice
{
    public AddAnnotationAdvice(
        IAspectInstanceInternal aspect,
        TemplateClassInstance template,
        IDeclaration targetDeclaration,
        ICompilation sourceCompilation,
        AnnotationInstance annotationInstance ) :
        base( aspect, template, targetDeclaration, sourceCompilation, null )
    {
        this.AnnotationInstance = annotationInstance;
    }

    public override AdviceKind AdviceKind => AdviceKind.AddAnnotation;

    public AnnotationInstance AnnotationInstance { get; }

    public override AdviceImplementationResult Implement(
        ProjectServiceProvider serviceProvider,
        CompilationModel compilation,
        Action<ITransformation> addTransformation )
    {
        addTransformation( new AddAnnotationTransformation( this, this.TargetDeclaration.GetTarget( compilation ), this.AnnotationInstance ) );

        return AdviceImplementationResult.Success();
    }
}