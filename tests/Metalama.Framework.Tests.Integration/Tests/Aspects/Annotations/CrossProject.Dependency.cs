#if TEST_OPTIONS
// @Include(_Common.cs)
#endif

using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Annotations.CrossProject;

public class AddAnnotationAspect : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        builder.Advice.AddAnnotation( builder.Target, new MyAnnotation( "TheValue" ), true );
    }
}

public class MyAnnotation : IAnnotation<INamedType>
{
    public MyAnnotation( string? value )
    {
        Value = value;
    }

    public string? Value { get; }
}

[AddAnnotationAspect]
public class BaseClass { }