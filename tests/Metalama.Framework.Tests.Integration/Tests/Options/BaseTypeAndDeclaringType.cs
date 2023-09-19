using Metalama.Framework.Tests.Integration.Tests.Options;
#if TEST_OPTIONS
// @Include(_Common.cs)
#endif

namespace Metalama.Framework.Tests.Integration.Tests.Options.BaseTypeAndDeclaringType;

[MyOptions( "Attribute.C" )]
[ShowOptionsAspect]
public class C { }

[MyOptions( "Attribute.P" )]
[ShowOptionsAspect]
public class P
{
    // <target>
    [ShowOptionsAspect]
    public class D : C { }
}