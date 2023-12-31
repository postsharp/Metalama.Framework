#if TEST_OPTIONS
// @Include(_Common.cs)
#endif

using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Tests.Integration.Tests.Aspects.Annotations.AddAnnotation;

[assembly: AspectOrder( typeof(ReadAnnotationAspect), typeof(AddAnnotationAspect) )]

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Annotations.AddAnnotation;

public class AddAnnotationAspect : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        builder.Advice.AddAnnotation( builder.Target, new MyAnnotation( "TheValue" ) );
    }
}

public class ReadAnnotationAspect : TypeAspect
{
    [Introduce]
    public string? TheAnnotation = meta.Target.Type.Enhancements().GetAnnotations<MyAnnotation>().Single().Value;
}

// <target>
[AddAnnotationAspect]
[ReadAnnotationAspect]
internal class C { }