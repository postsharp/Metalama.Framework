// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.CodeModel;

internal readonly struct AnnotationInstance
{
    public IAnnotation Annotation { get; }

    public bool Export { get; }

    public AnnotationInstance( IAnnotation annotation, bool export )
    {
        this.Annotation = annotation;
        this.Export = export;
    }
}

public interface IExternalAnnotationProvider
{
    ImmutableArray<IAnnotation> GetAnnotations( IDeclaration declaration );
}