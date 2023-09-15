using Metalama.Framework.Tests.Integration.Tests.Options;
#if TEST_OPTIONS
// @Include(_Common.cs)
#endif

namespace Metalama.Framework.Tests.Integration.Tests.Options.BaseTypeAndDeclaringType;

[MyOptions( "Attribute.C" )]
[OptionsAspect]
public class C { }

[MyOptions( "Attribute.P" )]
[OptionsAspect]
public class P
{
    // <target>
    [OptionsAspect]
    public class D : C { }
}