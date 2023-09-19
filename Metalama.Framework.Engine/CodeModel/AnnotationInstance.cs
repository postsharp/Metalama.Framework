// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.References;

namespace Metalama.Framework.Engine.CodeModel;

public readonly struct AnnotationInstance
{
    public Ref<IDeclaration> TargetDeclaration { get; }

    public IAnnotation Annotation { get; }

    public bool Export { get; }

    public AnnotationInstance( IAnnotation annotation, bool export )
    {
        this.Annotation = annotation;
        this.Export = export;
    }
}