// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;

namespace Metalama.Framework.Engine.CodeModel;

internal sealed class AnnotationInstance
{
    public IAnnotation Annotation { get; }

    public bool Export { get; }

    public AnnotationInstance( IAnnotation annotation, bool export )
    {
        this.Annotation = annotation;
        this.Export = export;
    }
}