#if TEST_OPTIONS
// @RequiredConstant(ROSLYN_4_4_0_OR_GREATER)
#endif

#if ROSLYN_4_4_0_OR_GREATER

using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.CSharp11.GenericAspect_NotSupported;

public class GenericAspect<T> : TypeAspect
{
    public GenericAspect( T defaultValue ) { }

    [Introduce]
    public T? IntroducedProperty { get; set; }
}

// <target>
[GenericAspect<int>( 5 )]
public class C { }

#endif