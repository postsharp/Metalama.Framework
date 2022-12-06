using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.CSharp11.GenericAspect_NotSupported;

public class GenericAspect<T> : TypeAspect
{
    public GenericAspect( T defaultValue )
    {
        IntroducedProperty = defaultValue;
    }

    [Introduce]
    public T IntroducedProperty { get; set; }
}

// <target>
[GenericAspect<int>( 5 )]
public class C { }