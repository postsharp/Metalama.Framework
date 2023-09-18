#if TEST_OPTIONS
// @Include(_Common.cs)
#endif

using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Tests.Integration.Tests.Aspects.Annotations.AddAnnotation;

[assembly: AspectOrder( typeof(Aspect2), typeof(Aspect1) )]

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Annotations.AddAnnotation;

public class Aspect1 : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        builder.Advice.AddAnnotation( builder.Target, new MyAnnotation( "TheValue" ) );
    }
}

public class Aspect2 : TypeAspect
{
    [Introduce]
    public string? TheAnnotation = meta.Target.Type.Enhancements().GetAnnotations<MyAnnotation>().Single().Value;
}

// <target>
[Aspect1]
[Aspect2]
internal class C { }